using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Orleans;
using UbntSecPilot.Agents.Orleans;
using UbntSecPilot.Application.Services;
using UbntSecPilot.Domain.Models;
using UbntSecPilot.Domain.Repositories;
using UbntSecPilot.Domain.ValueObjects;
using Xunit;

namespace UbntSecPilot.Agents.Orleans.Tests
{
    public class ThreatEnrichmentAgentGrainTests
    {
        private readonly Mock<INetworkEventRepository> _eventRepoMock;
        private readonly Mock<IThreatFindingRepository> _findingRepoMock;
        private readonly Mock<IAgentDecisionRepository> _decisionRepoMock;
        private readonly Mock<PreAnalysisService> _preAnalysisMock;
        private readonly Mock<ILogger<ThreatEnrichmentAgentGrain>> _loggerMock;
        private readonly Mock<IConfiguration> _configMock;
        private readonly Mock<IGrainFactory> _grainFactoryMock;
        private readonly Mock<ITransactionCoordinatorGrain> _coordinatorMock;

        public ThreatEnrichmentAgentGrainTests()
        {
            _eventRepoMock = new Mock<INetworkEventRepository>();
            _findingRepoMock = new Mock<IThreatFindingRepository>();
            _decisionRepoMock = new Mock<IAgentDecisionRepository>();
            _preAnalysisMock = new Mock<PreAnalysisService>();
            _loggerMock = new Mock<ILogger<ThreatEnrichmentAgentGrain>>();
            _configMock = new Mock<IConfiguration>();
            _grainFactoryMock = new Mock<IGrainFactory>();
            _coordinatorMock = new Mock<ITransactionCoordinatorGrain>();

            // Setup default configuration
            _configMock.Setup(c => c.GetValue<bool>("Agents:Use2PC")).Returns(true);
            _grainFactoryMock.Setup(gf => gf.GetGrain<ITransactionCoordinatorGrain>(It.IsAny<string>(), null))
                .Returns(_coordinatorMock.Object);
            _coordinatorMock.Setup(c => c.RunTwoPhaseCommitAsync(It.IsAny<TransactionPayload>(), It.IsAny<List<string>>()))
                .ReturnsAsync(true);
        }

        [Fact]
        public async Task ExecuteAsync_WithNoEvents_ReturnsNoEventsResult()
        {
            // Arrange
            _eventRepoMock.Setup(r => r.GetUnprocessedEventsAsync(50))
                .ReturnsAsync(new List<NetworkEvent>());

            var grain = CreateGrain();

            // Act
            var result = await grain.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.Equal("threat-enrichment", result.Action);
            Assert.Contains("processed 0 events", result.Reason);
            Assert.Equal(0, result.Metadata["events_collected"]);
            Assert.Equal(0, result.Metadata["findings_produced"]);
            Assert.Equal(0, result.Metadata["events_processed"]);

            _loggerMock.Verify(l => l.LogInformation("No events to process"), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WithCleanEvent_ProcessesAndSavesVia2PC()
        {
            // Arrange
            var cleanEvent = CreateCleanNetworkEvent();
            _eventRepoMock.Setup(r => r.GetUnprocessedEventsAsync(50))
                .ReturnsAsync(new List<NetworkEvent> { cleanEvent });
            _preAnalysisMock.Setup(p => p.AnalyzeAsync(cleanEvent))
                .ReturnsAsync((false, "clean", new Dictionary<string, object>()));

            var grain = CreateGrain();

            // Act
            var result = await grain.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.Equal("threat-enrichment", result.Action);
            Assert.Contains("processed 1 events", result.Reason);
            Assert.Equal(1, result.Metadata["events_collected"]);
            Assert.Equal(1, result.Metadata["events_processed"]);
            Assert.Equal(0, result.Metadata["findings_produced"]);

            // Verify 2PC was called for the event
            _coordinatorMock.Verify(c => c.RunTwoPhaseCommitAsync(
                It.Is<TransactionPayload>(p => p.UpdatedEvent == cleanEvent && p.Finding == null),
                It.Is<List<string>>(participants => participants.Contains($"event:{cleanEvent.EventId}"))),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WithSuspiciousEvent_CreatesFindingAndSavesVia2PC()
        {
            // Arrange
            var suspiciousEvent = CreateSuspiciousNetworkEvent();
            _eventRepoMock.Setup(r => r.GetUnprocessedEventsAsync(50))
                .ReturnsAsync(new List<NetworkEvent> { suspiciousEvent });
            _preAnalysisMock.Setup(p => p.AnalyzeAsync(suspiciousEvent))
                .ReturnsAsync((true, "suspicious", new Dictionary<string, object> { ["signal"] = "test" }));

            var grain = CreateGrain();

            // Act
            var result = await grain.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.Equal("threat-enrichment", result.Action);
            Assert.Contains("processed 1 events", result.Reason);
            Assert.Equal(1, result.Metadata["events_collected"]);
            Assert.Equal(1, result.Metadata["events_processed"]);
            Assert.Equal(1, result.Metadata["findings_produced"]);

            // Verify 2PC was called for both event and finding
            _coordinatorMock.Verify(c => c.RunTwoPhaseCommitAsync(
                It.Is<TransactionPayload>(p => p.UpdatedEvent == suspiciousEvent && p.Finding != null),
                It.Is<List<string>>(participants =>
                    participants.Contains($"event:{suspiciousEvent.EventId}") &&
                    participants.Contains($"finding:{suspiciousEvent.EventId}"))),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_When2PCDisabled_UsesDirectSave()
        {
            // Arrange
            _configMock.Setup(c => c.GetValue<bool>("Agents:Use2PC")).Returns(false);

            var suspiciousEvent = CreateSuspiciousNetworkEvent();
            _eventRepoMock.Setup(r => r.GetUnprocessedEventsAsync(50))
                .ReturnsAsync(new List<NetworkEvent> { suspiciousEvent });
            _preAnalysisMock.Setup(p => p.AnalyzeAsync(suspiciousEvent))
                .ReturnsAsync((true, "suspicious", new Dictionary<string, object>()));
            _eventRepoMock.Setup(r => r.SaveAsync(suspiciousEvent))
                .Returns(Task.CompletedTask);
            _findingRepoMock.Setup(r => r.SaveManyAsync(It.IsAny<IEnumerable<ThreatFinding>>()))
                .Returns(Task.CompletedTask);
            _decisionRepoMock.Setup(r => r.SaveAsync(It.IsAny<AgentDecision>()))
                .Returns(Task.CompletedTask);

            var grain = CreateGrain();

            // Act
            var result = await grain.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.Equal("threat-enrichment", result.Action);
            Assert.Contains("processed 1 events", result.Reason);

            // Verify direct save was used instead of 2PC
            _eventRepoMock.Verify(r => r.SaveAsync(suspiciousEvent), Times.Once);
            _findingRepoMock.Verify(r => r.SaveManyAsync(It.IsAny<IEnumerable<ThreatFinding>>()), Times.Once);
            _decisionRepoMock.Verify(r => r.SaveAsync(It.IsAny<AgentDecision>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WithMultipleEvents_ProcessesAllAndCreatesSummary()
        {
            // Arrange
            var events = new List<NetworkEvent>
            {
                CreateCleanNetworkEvent(),
                CreateSuspiciousNetworkEvent(),
                CreateSuspiciousNetworkEvent()
            };

            _eventRepoMock.Setup(r => r.GetUnprocessedEventsAsync(50))
                .ReturnsAsync(events);
            _preAnalysisMock.Setup(p => p.AnalyzeAsync(It.IsAny<NetworkEvent>()))
                .ReturnsAsync((true, "suspicious", new Dictionary<string, object>()));

            var grain = CreateGrain();

            // Act
            var result = await grain.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.Equal("threat-enrichment", result.Action);
            Assert.Contains("processed 3 events", result.Reason);
            Assert.Equal(3, result.Metadata["events_collected"]);
            Assert.Equal(3, result.Metadata["events_processed"]);
            Assert.Equal(2, result.Metadata["findings_produced"]);

            // Verify threat severity breakdown is included
            Assert.True(result.Metadata.ContainsKey("threat_severity_breakdown"));
            var breakdown = result.Metadata["threat_severity_breakdown"] as Dictionary<string, int>;
            Assert.NotNull(breakdown);
            Assert.Equal(2, breakdown["high"]); // Both suspicious events should be "high" severity
        }

        private ThreatEnrichmentAgentGrain CreateGrain()
        {
            return new ThreatEnrichmentAgentGrain(
                _eventRepoMock.Object,
                _findingRepoMock.Object,
                _decisionRepoMock.Object,
                _preAnalysisMock.Object,
                _loggerMock.Object,
                _configMock.Object);
        }

        private NetworkEvent CreateCleanNetworkEvent()
        {
            return new NetworkEvent(
                "clean-event-1",
                "test-source",
                new Dictionary<string, object>
                {
                    ["source_ip"] = "192.168.1.100",
                    ["destination_port"] = 80,
                    ["user_agent"] = "Mozilla/5.0"
                },
                DateTime.UtcNow);
        }

        private NetworkEvent CreateSuspiciousNetworkEvent()
        {
            return new NetworkEvent(
                "suspicious-event-1",
                "test-source",
                new Dictionary<string, object>
                {
                    ["source_ip"] = "192.168.1.100",
                    ["destination_port"] = 31337,
                    ["user_agent"] = "malicious-bot-scanner"
                },
                DateTime.UtcNow);
        }
    }
}
