using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using UbntSecPilot.Agents;
using UbntSecPilot.Domain.Models;
using UbntSecPilot.Domain.Repositories;
using UbntSecPilot.Domain.ValueObjects;
using Xunit;

namespace UbntSecPilot.Agents.Tests
{
    public class ThreatEnrichmentAgentTests
    {
        private readonly Mock<INetworkEventRepository> _eventRepoMock;
        private readonly Mock<IThreatFindingRepository> _findingRepoMock;
        private readonly Mock<IAgentDecisionRepository> _decisionRepoMock;
        private readonly Mock<ILogger<ThreatEnrichmentAgent>> _loggerMock;

        public ThreatEnrichmentAgentTests()
        {
            _eventRepoMock = new Mock<INetworkEventRepository>();
            _findingRepoMock = new Mock<IThreatFindingRepository>();
            _decisionRepoMock = new Mock<IAgentDecisionRepository>();
            _loggerMock = new Mock<ILogger<ThreatEnrichmentAgent>>();
        }

        [Fact]
        public async Task RunAsync_WithNoEvents_ReturnsNoEventsResult()
        {
            // Arrange
            _eventRepoMock.Setup(r => r.GetUnprocessedEventsAsync(50))
                .ReturnsAsync(new List<NetworkEvent>());

            var agent = CreateAgent();

            // Act
            var result = await agent.RunAsync();

            // Assert
            Assert.Equal("threat-enrichment", result.Action);
            Assert.Equal("no events", result.Reason);
            Assert.Equal(0, result.Metadata["events_collected"]);
            Assert.Equal(0, result.Metadata["findings_produced"]);

            _loggerMock.Verify(l => l.LogInformation("No events to process"), Times.Once);
        }

        [Fact]
        public async Task RunAsync_WithCleanEvents_ProcessesAndMarksAsProcessed()
        {
            // Arrange
            var cleanEvent = CreateCleanNetworkEvent();
            _eventRepoMock.Setup(r => r.GetUnprocessedEventsAsync(50))
                .ReturnsAsync(new List<NetworkEvent> { cleanEvent });
            _eventRepoMock.Setup(r => r.SaveAsync(cleanEvent))
                .Returns(Task.CompletedTask);

            var agent = CreateAgent();

            // Act
            var result = await agent.RunAsync();

            // Assert
            Assert.Equal("threat-enrichment", result.Action);
            Assert.Contains("processed 1 events", result.Reason);
            Assert.Equal(1, result.Metadata["events_collected"]);
            Assert.Equal(0, result.Metadata["findings_produced"]);

            _eventRepoMock.Verify(r => r.SaveAsync(cleanEvent), Times.Once);
            _decisionRepoMock.Verify(r => r.SaveAsync(It.Is<AgentDecision>(d =>
                d.AgentName == "threat-enrichment" && d.Reason.Contains("processed 1 events"))), Times.Once);
        }

        [Fact]
        public async Task RunAsync_WithSuspiciousEvents_CreatesFindings()
        {
            // Arrange
            var suspiciousEvent = CreateSuspiciousNetworkEvent();
            _eventRepoMock.Setup(r => r.GetUnprocessedEventsAsync(50))
                .ReturnsAsync(new List<NetworkEvent> { suspiciousEvent });
            _eventRepoMock.Setup(r => r.SaveAsync(suspiciousEvent))
                .Returns(Task.CompletedTask);
            _findingRepoMock.Setup(r => r.SaveAsync(It.IsAny<ThreatFinding>()))
                .Returns(Task.CompletedTask);

            var agent = CreateAgent();

            // Act
            var result = await agent.RunAsync();

            // Assert
            Assert.Equal("threat-enrichment", result.Action);
            Assert.Contains("processed 1 events", result.Reason);
            Assert.Equal(1, result.Metadata["events_collected"]);
            Assert.Equal(1, result.Metadata["findings_produced"]);

            _findingRepoMock.Verify(r => r.SaveAsync(It.Is<ThreatFinding>(f =>
                f.EventId == suspiciousEvent.EventId &&
                f.Severity == "high")), Times.Once);
        }

        [Fact]
        public async Task RunAsync_WithMultipleEvents_ProcessesAllEvents()
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
            _eventRepoMock.Setup(r => r.SaveAsync(It.IsAny<NetworkEvent>()))
                .Returns(Task.CompletedTask);
            _findingRepoMock.Setup(r => r.SaveAsync(It.IsAny<ThreatFinding>()))
                .Returns(Task.CompletedTask);

            var agent = CreateAgent();

            // Act
            var result = await agent.RunAsync();

            // Assert
            Assert.Equal("threat-enrichment", result.Action);
            Assert.Contains("processed 3 events", result.Reason);
            Assert.Equal(3, result.Metadata["events_collected"]);
            Assert.Equal(2, result.Metadata["findings_produced"]);

            _eventRepoMock.Verify(r => r.SaveAsync(It.IsAny<NetworkEvent>()), Times.Exactly(3));
            _findingRepoMock.Verify(r => r.SaveAsync(It.IsAny<ThreatFinding>()), Times.Exactly(2));
        }

        private ThreatEnrichmentAgent CreateAgent()
        {
            return new ThreatEnrichmentAgent(
                _eventRepoMock.Object,
                _findingRepoMock.Object,
                _decisionRepoMock.Object,
                _loggerMock.Object);
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
