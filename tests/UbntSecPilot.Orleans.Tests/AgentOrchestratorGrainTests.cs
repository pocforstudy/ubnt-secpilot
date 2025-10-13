using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Orleans;
using UbntSecPilot.Agents;
using UbntSecPilot.Agents.Orleans;
using Xunit;

namespace UbntSecPilot.Agents.Orleans.Tests
{
    public class AgentOrchestratorGrainTests
    {
        private readonly Mock<ILogger<AgentOrchestratorGrain>> _loggerMock;
        private readonly Mock<IGrainFactory> _grainFactoryMock;
        private readonly Mock<IAgentGrain> _agentGrainMock;

        public AgentOrchestratorGrainTests()
        {
            _loggerMock = new Mock<ILogger<AgentOrchestratorGrain>>();
            _grainFactoryMock = new Mock<IGrainFactory>();
            _agentGrainMock = new Mock<IAgentGrain>();

            _grainFactoryMock.Setup(gf => gf.GetGrain<IAgentGrain>("threat-enrichment", null))
                .Returns(_agentGrainMock.Object);
        }

        [Fact]
        public async Task GetAvailableAgentsAsync_ReturnsConfiguredAgents()
        {
            // Arrange
            var orchestrator = new AgentOrchestratorGrain(_loggerMock.Object, _grainFactoryMock.Object);

            // Act
            var agents = await orchestrator.GetAvailableAgentsAsync();

            // Assert
            Assert.Single(agents);
            Assert.Contains("threat-enrichment", agents);
        }

        [Fact]
        public async Task GetAgentStatusAsync_WithRunningAgent_ReturnsRunningStatus()
        {
            // Arrange
            _agentGrainMock.Setup(g => g.IsRunning()).ReturnsAsync(true);
            _agentGrainMock.Setup(g => g.GetStatus()).ReturnsAsync("running");

            var orchestrator = new AgentOrchestratorGrain(_loggerMock.Object, _grainFactoryMock.Object);

            // Act
            var status = await orchestrator.GetAgentStatusAsync("threat-enrichment");

            // Assert
            Assert.Equal("threat-enrichment", status.AgentName);
            Assert.Equal("running", status.Status);
            Assert.True(status.IsRunning);
        }

        [Fact]
        public async Task GetAgentStatusAsync_WithIdleAgent_ReturnsIdleStatus()
        {
            // Arrange
            _agentGrainMock.Setup(g => g.IsRunning()).ReturnsAsync(false);
            _agentGrainMock.Setup(g => g.GetStatus()).ReturnsAsync("idle");

            var orchestrator = new AgentOrchestratorGrain(_loggerMock.Object, _grainFactoryMock.Object);

            // Act
            var status = await orchestrator.GetAgentStatusAsync("threat-enrichment");

            // Assert
            Assert.Equal("threat-enrichment", status.AgentName);
            Assert.Equal("idle", status.Status);
            Assert.False(status.IsRunning);
        }

        [Fact]
        public async Task RunAgentAsync_WithValidAgent_ReturnsAgentResult()
        {
            // Arrange
            var expectedResult = new AgentResult("threat-enrichment", "completed", new Dictionary<string, object>());
            _agentGrainMock.Setup(g => g.RunAsync(CancellationToken.None))
                .ReturnsAsync(expectedResult);

            var orchestrator = new AgentOrchestratorGrain(_loggerMock.Object, _grainFactoryMock.Object);

            // Act
            var result = await orchestrator.RunAgentAsync("threat-enrichment");

            // Assert
            Assert.Equal(expectedResult.Action, result.Action);
            Assert.Equal(expectedResult.Reason, result.Reason);
            Assert.Equal(expectedResult.Metadata, result.Metadata);

            _loggerMock.Verify(l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, _) => state.ToString()!.Contains("Running agent")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
            _agentGrainMock.Verify(g => g.RunAsync(CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task RunAgentAsync_WithCancellationToken_PassesTokenToAgent()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            var expectedResult = new AgentResult("threat-enrichment", "cancelled", new Dictionary<string, object>());
            _agentGrainMock.Setup(g => g.RunAsync(cts.Token))
                .ReturnsAsync(expectedResult);

            var orchestrator = new AgentOrchestratorGrain(_loggerMock.Object, _grainFactoryMock.Object);

            // Act
            var result = await orchestrator.RunAgentAsync("threat-enrichment", cts.Token);

            // Assert
            Assert.Equal("threat-enrichment", result.Action);
            Assert.Equal("cancelled", result.Reason);
            _agentGrainMock.Verify(g => g.RunAsync(cts.Token), Times.Once);
        }

        [Fact]
        public async Task RunAgentAsync_WithNonExistentAgent_ThrowsException()
        {
            // Arrange
            var orchestrator = new AgentOrchestratorGrain(_loggerMock.Object, _grainFactoryMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                orchestrator.RunAgentAsync("non-existent-agent"));
        }

        [Fact]
        public async Task GetAvailableAgentsAsync_AlwaysReturnsSameList()
        {
            // Arrange
            var orchestrator = new AgentOrchestratorGrain(_loggerMock.Object, _grainFactoryMock.Object);

            // Act
            var agents1 = await orchestrator.GetAvailableAgentsAsync();
            var agents2 = await orchestrator.GetAvailableAgentsAsync();

            // Assert
            Assert.Equal(agents1, agents2);
            Assert.Single(agents1);
            Assert.Contains("threat-enrichment", agents1);
        }
    }

    // Helper class for testing
    public class AgentStatus
    {
        public string AgentName { get; }
        public string Status { get; }
        public bool IsRunning { get; }
        public DateTime LastUpdated { get; }

        public AgentStatus(string agentName, string status, bool isRunning)
        {
            AgentName = agentName;
            Status = status;
            IsRunning = isRunning;
            LastUpdated = DateTime.UtcNow;
        }
    }
}
