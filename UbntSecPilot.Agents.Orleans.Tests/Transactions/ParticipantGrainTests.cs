using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using UbntSecPilot.Agents.Orleans.Transactions;
using UbntSecPilot.Domain.Models;
using UbntSecPilot.Domain.Repositories;
using Xunit;

namespace UbntSecPilot.Agents.Orleans.Tests.Transactions
{
    public class ParticipantGrainTests
    {
        private readonly Mock<ILogger<ParticipantGrain>> _loggerMock;
        private readonly Mock<INetworkEventRepository> _eventRepoMock;
        private readonly Mock<IThreatFindingRepository> _findingRepoMock;
        private readonly Mock<IAgentDecisionRepository> _decisionRepoMock;

        public ParticipantGrainTests()
        {
            _loggerMock = new Mock<ILogger<ParticipantGrain>>();
            _eventRepoMock = new Mock<INetworkEventRepository>();
            _findingRepoMock = new Mock<IThreatFindingRepository>();
            _decisionRepoMock = new Mock<IAgentDecisionRepository>();
        }

        [Fact]
        public async Task PrepareAsync_WithEventParticipant_ReturnsTrue()
        {
            // Arrange
            var networkEvent = new NetworkEvent("event1", "source", new Dictionary<string, object>(), DateTime.UtcNow);
            var payload = new TransactionPayload
            {
                TransactionId = "test-tx-1",
                UpdatedEvent = networkEvent
            };

            var participant = new ParticipantGrain(
                _loggerMock.Object,
                _eventRepoMock.Object,
                _findingRepoMock.Object,
                _decisionRepoMock.Object);

            // Act
            var result = await participant.PrepareAsync(payload);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task PrepareAsync_WithFindingParticipant_ReturnsTrue()
        {
            // Arrange
            var finding = new ThreatFinding("event1", "high", "test finding", new Dictionary<string, object>());
            var payload = new TransactionPayload
            {
                TransactionId = "test-tx-2",
                Finding = finding
            };

            var participant = new ParticipantGrain(
                _loggerMock.Object,
                _eventRepoMock.Object,
                _findingRepoMock.Object,
                _decisionRepoMock.Object);

            // Act
            var result = await participant.PrepareAsync(payload);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task PrepareAsync_WithDecisionParticipant_ReturnsTrue()
        {
            // Arrange
            var decision = new AgentDecision("test-agent", "test decision");
            var payload = new TransactionPayload
            {
                TransactionId = "test-tx-3",
                Decision = decision
            };

            var participant = new ParticipantGrain(
                _loggerMock.Object,
                _eventRepoMock.Object,
                _findingRepoMock.Object,
                _decisionRepoMock.Object);

            // Act
            var result = await participant.PrepareAsync(payload);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CommitAsync_WithEventParticipant_SavesEvent()
        {
            // Arrange
            var networkEvent = new NetworkEvent("event1", "source", new Dictionary<string, object>(), DateTime.UtcNow);
            var payload = new TransactionPayload
            {
                TransactionId = "test-tx-1",
                UpdatedEvent = networkEvent
            };

            _eventRepoMock.Setup(r => r.SaveAsync(networkEvent)).Returns(Task.CompletedTask);

            var participant = new ParticipantGrain(
                _loggerMock.Object,
                _eventRepoMock.Object,
                _findingRepoMock.Object,
                _decisionRepoMock.Object);

            // Act
            await participant.CommitAsync(payload.TransactionId);

            // Assert
            _eventRepoMock.Verify(r => r.SaveAsync(networkEvent), Times.Once);
        }

        [Fact]
        public async Task CommitAsync_WithFindingParticipant_SavesFinding()
        {
            // Arrange
            var finding = new ThreatFinding("event1", "high", "test finding", new Dictionary<string, object>());
            var payload = new TransactionPayload
            {
                TransactionId = "test-tx-2",
                Finding = finding
            };

            _findingRepoMock.Setup(r => r.SaveAsync(finding)).Returns(Task.CompletedTask);

            var participant = new ParticipantGrain(
                _loggerMock.Object,
                _eventRepoMock.Object,
                _findingRepoMock.Object,
                _decisionRepoMock.Object);

            // Act
            await participant.CommitAsync(payload.TransactionId);

            // Assert
            _findingRepoMock.Verify(r => r.SaveAsync(finding), Times.Once);
        }

        [Fact]
        public async Task CommitAsync_WithDecisionParticipant_SavesDecision()
        {
            // Arrange
            var decision = new AgentDecision("test-agent", "test decision");
            var payload = new TransactionPayload
            {
                TransactionId = "test-tx-3",
                Decision = decision
            };

            _decisionRepoMock.Setup(r => r.SaveAsync(decision)).Returns(Task.CompletedTask);

            var participant = new ParticipantGrain(
                _loggerMock.Object,
                _eventRepoMock.Object,
                _findingRepoMock.Object,
                _decisionRepoMock.Object);

            // Act
            await participant.CommitAsync(payload.TransactionId);

            // Assert
            _decisionRepoMock.Verify(r => r.SaveAsync(decision), Times.Once);
        }

        [Fact]
        public async Task AbortAsync_WithEventParticipant_DoesNotSave()
        {
            // Arrange
            var payload = new TransactionPayload
            {
                TransactionId = "test-tx-1",
                UpdatedEvent = new NetworkEvent("event1", "source", new Dictionary<string, object>(), DateTime.UtcNow)
            };

            var participant = new ParticipantGrain(
                _loggerMock.Object,
                _eventRepoMock.Object,
                _findingRepoMock.Object,
                _decisionRepoMock.Object);

            // Act
            await participant.AbortAsync(payload.TransactionId);

            // Assert - No repositories should be called during abort
            _eventRepoMock.Verify(r => r.SaveAsync(It.IsAny<NetworkEvent>()), Times.Never);
            _findingRepoMock.Verify(r => r.SaveAsync(It.IsAny<ThreatFinding>()), Times.Never);
            _decisionRepoMock.Verify(r => r.SaveAsync(It.IsAny<AgentDecision>()), Times.Never);
        }

        [Fact]
        public async Task PrepareAsync_WithNullPayload_ReturnsFalse()
        {
            // Arrange
            var participant = new ParticipantGrain(
                _loggerMock.Object,
                _eventRepoMock.Object,
                _findingRepoMock.Object,
                _decisionRepoMock.Object);

            // Act
            var result = await participant.PrepareAsync(null!);

            // Assert
            Assert.False(result);
        }
    }
}
