using System;
using System.Collections.Generic;
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
    public class AgentGrainTests
    {
        private readonly Mock<ILogger<TestAgentGrain>> _loggerMock;
        private readonly TestableAgentGrain _grain;

        public AgentGrainTests()
        {
            _loggerMock = new Mock<ILogger<TestAgentGrain>>();
            _grain = new TestableAgentGrain(_loggerMock.Object);
        }

        [Fact]
        public async Task OnActivateAsync_SetsInitialState()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;

            // Act
            await _grain.OnActivateAsync(cancellationToken);

            // Assert
            Assert.Equal("idle", await _grain.GetStatus());
            Assert.False(await _grain.IsRunning());
            _loggerMock.Verify(l => l.LogInformation("Agent {GrainId} activated", "test-grain"), Times.Once);
        }

        [Fact]
        public async Task OnDeactivateAsync_LogsDeactivation()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;

            // Act
            await _grain.OnDeactivateAsync(DeactivationReason.Shutdown, cancellationToken);

            // Assert
            _loggerMock.Verify(l => l.LogInformation("Agent {GrainId} deactivated", "test-grain"), Times.Once);
        }

        [Fact]
        public async Task RunAsync_WhenNotRunning_ExecutesSuccessfully()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _grain.RunAsync(cancellationToken);

            // Assert
            Assert.Equal("test-agent", result.Action);
            Assert.Equal("completed", result.Reason);
            Assert.Equal("completed", await _grain.GetStatus());
            Assert.False(await _grain.IsRunning());
        }

        [Fact]
        public async Task RunAsync_WhenAlreadyRunning_ReturnsAlreadyRunningResult()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;

            // Start first execution
            var firstTask = _grain.RunAsync(cancellationToken);

            // Try to run again while first is still executing
            var secondResult = await _grain.RunAsync(cancellationToken);

            // Complete first execution
            await firstTask;

            // Assert
            Assert.Equal("agent", secondResult.Action);
            Assert.Equal("already running", secondResult.Reason);
            Assert.True(secondResult.Metadata.ContainsKey("error"));
        }

        [Fact]
        public async Task RunAsync_WithCancellation_ReturnsCancelledResult()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act
            var result = await _grain.RunAsync(cts.Token);

            // Assert
            Assert.Equal("agent", result.Action);
            Assert.Equal("cancelled", result.Reason);
            Assert.Equal("cancelled", await _grain.GetStatus());
        }

        [Fact]
        public async Task RunAsync_WithException_ReturnsFailedResult()
        {
            // Arrange
            var failingGrain = new FailingTestAgentGrain(_loggerMock.Object);
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await failingGrain.RunAsync(cancellationToken);

            // Assert
            Assert.Equal("agent", result.Action);
            Assert.Equal("failed", result.Reason);
            Assert.True(result.Metadata.ContainsKey("error"));
            Assert.Equal("failed", await failingGrain.GetStatus());
            Assert.False(await failingGrain.IsRunning());
        }

        private class TestableAgentGrain : AgentGrain
        {
            public TestableAgentGrain(ILogger logger) : base(logger)
            {
            }

            public override Task OnActivateAsync(CancellationToken cancellationToken)
            {
                this.GetPrimaryKeyString = () => "test-grain";
                return base.OnActivateAsync(cancellationToken);
            }

            protected override async Task<AgentResult> ExecuteAsync(CancellationToken cancellationToken)
            {
                await Task.Delay(10, cancellationToken);
                return new AgentResult("test-agent", "completed", new Dictionary<string, object>());
            }
        }

        private class FailingTestAgentGrain : AgentGrain
        {
            public FailingTestAgentGrain(ILogger logger) : base(logger)
            {
            }

            public override Task OnActivateAsync(CancellationToken cancellationToken)
            {
                this.GetPrimaryKeyString = () => "test-grain";
                return base.OnActivateAsync(cancellationToken);
            }

            protected override async Task<AgentResult> ExecuteAsync(CancellationToken cancellationToken)
            {
                await Task.Delay(10, cancellationToken);
                throw new InvalidOperationException("Test exception");
            }
        }
    }
}
