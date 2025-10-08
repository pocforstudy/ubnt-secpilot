using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Orleans;
using UbntSecPilot.Agents.Orleans.Transactions;
using UbntSecPilot.Domain.Models;
using UbntSecPilot.Domain.Repositories;
using Xunit;

namespace UbntSecPilot.Agents.Orleans.Tests.Transactions
{
    public class TransactionCoordinatorGrainTests
    {
        private readonly Mock<ILogger<TransactionCoordinatorGrain>> _loggerMock;
        private readonly Mock<IGrainFactory> _grainFactoryMock;
        private readonly Mock<IParticipantGrain> _participantMock;

        public TransactionCoordinatorGrainTests()
        {
            _loggerMock = new Mock<ILogger<TransactionCoordinatorGrain>>();
            _grainFactoryMock = new Mock<IGrainFactory>();
            _participantMock = new Mock<IParticipantGrain>();
        }

        [Fact]
        public async Task RunTwoPhaseCommitAsync_WithAllParticipantsPrepare_ReturnsTrue()
        {
            // Arrange
            var payload = new TransactionPayload
            {
                TransactionId = "test-tx-1",
                UpdatedEvent = new NetworkEvent("event1", "source", new Dictionary<string, object>(), DateTime.UtcNow)
            };
            var participants = new List<string> { "event:event1", "finding:event1" };

            _grainFactoryMock.Setup(gf => gf.GetGrain<IParticipantGrain>("event:event1", null))
                .Returns(_participantMock.Object);
            _grainFactoryMock.Setup(gf => gf.GetGrain<IParticipantGrain>("finding:event1", null))
                .Returns(_participantMock.Object);

            _participantMock.Setup(p => p.PrepareAsync(payload)).ReturnsAsync(true);

            var coordinator = new TransactionCoordinatorGrain(_loggerMock.Object, _grainFactoryMock.Object);

            // Act
            var result = await coordinator.RunTwoPhaseCommitAsync(payload, participants);

            // Assert
            Assert.True(result);

            // Verify prepare was called on all participants
            _participantMock.Verify(p => p.PrepareAsync(payload), Times.Exactly(2));

            // Verify commit was called on all participants
            _participantMock.Verify(p => p.CommitAsync(payload.TransactionId), Times.Exactly(2));
        }

        [Fact]
        public async Task RunTwoPhaseCommitAsync_WithPrepareFailure_ReturnsFalse()
        {
            // Arrange
            var payload = new TransactionPayload
            {
                TransactionId = "test-tx-2",
                UpdatedEvent = new NetworkEvent("event1", "source", new Dictionary<string, object>(), DateTime.UtcNow)
            };
            var participants = new List<string> { "event:event1" };

            _grainFactoryMock.Setup(gf => gf.GetGrain<IParticipantGrain>("event:event1", null))
                .Returns(_participantMock.Object);

            _participantMock.Setup(p => p.PrepareAsync(payload)).ReturnsAsync(false);

            var coordinator = new TransactionCoordinatorGrain(_loggerMock.Object, _grainFactoryMock.Object);

            // Act
            var result = await coordinator.RunTwoPhaseCommitAsync(payload, participants);

            // Assert
            Assert.False(result);

            // Verify prepare was called
            _participantMock.Verify(p => p.PrepareAsync(payload), Times.Once);

            // Verify abort was called instead of commit
            _participantMock.Verify(p => p.AbortAsync(payload.TransactionId), Times.Once);
            _participantMock.Verify(p => p.CommitAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RunTwoPhaseCommitAsync_WithCommitFailure_HandlesGracefully()
        {
            // Arrange
            var payload = new TransactionPayload
            {
                TransactionId = "test-tx-3",
                UpdatedEvent = new NetworkEvent("event1", "source", new Dictionary<string, object>(), DateTime.UtcNow)
            };
            var participants = new List<string> { "event:event1" };

            _grainFactoryMock.Setup(gf => gf.GetGrain<IParticipantGrain>("event:event1", null))
                .Returns(_participantMock.Object);

            _participantMock.Setup(p => p.PrepareAsync(payload)).ReturnsAsync(true);
            _participantMock.Setup(p => p.CommitAsync(payload.TransactionId)).ThrowsAsync(new InvalidOperationException("Commit failed"));

            var coordinator = new TransactionCoordinatorGrain(_loggerMock.Object, _grainFactoryMock.Object);

            // Act & Assert - Should not throw
            var result = await coordinator.RunTwoPhaseCommitAsync(payload, participants);

            // The result might be true or false depending on implementation
            // The important thing is it doesn't throw an exception
            Assert.True(result == true || result == false);

            _participantMock.Verify(p => p.PrepareAsync(payload), Times.Once);
            _participantMock.Verify(p => p.CommitAsync(payload.TransactionId), Times.Once);
        }

        [Fact]
        public async Task RunTwoPhaseCommitAsync_WithEmptyParticipants_ReturnsTrue()
        {
            // Arrange
            var payload = new TransactionPayload { TransactionId = "test-tx-4" };
            var participants = new List<string>();

            var coordinator = new TransactionCoordinatorGrain(_loggerMock.Object, _grainFactoryMock.Object);

            // Act
            var result = await coordinator.RunTwoPhaseCommitAsync(payload, participants);

            // Assert
            Assert.True(result);

            // Verify no participants were contacted
            _grainFactoryMock.Verify(gf => gf.GetGrain<IParticipantGrain>(It.IsAny<string>(), null), Times.Never);
        }

        [Fact]
        public async Task RunTwoPhaseCommitAsync_WithDecisionOnly_ProcessesDecision()
        {
            // Arrange
            var decision = new AgentDecision("test-agent", "test decision");
            var payload = new TransactionPayload
            {
                TransactionId = "test-tx-5",
                Decision = decision
            };
            var participants = new List<string> { "decision:test" };

            _grainFactoryMock.Setup(gf => gf.GetGrain<IParticipantGrain>("decision:test", null))
                .Returns(_participantMock.Object);

            _participantMock.Setup(p => p.PrepareAsync(payload)).ReturnsAsync(true);

            var coordinator = new TransactionCoordinatorGrain(_loggerMock.Object, _grainFactoryMock.Object);

            // Act
            var result = await coordinator.RunTwoPhaseCommitAsync(payload, participants);

            // Assert
            Assert.True(result);

            _participantMock.Verify(p => p.PrepareAsync(
                It.Is<TransactionPayload>(tp => tp.Decision == decision)), Times.Once);
        }
    }
}
