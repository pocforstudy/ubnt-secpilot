using System;
using System.Collections.Generic;
using System.Threading;
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
    public class BaseAgentTests
    {
        [Fact]
        public async Task RunAsync_WithSuccessfulExecution_ReturnsSuccessResult()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<TestAgent>>();
            var agent = new TestAgent(mockLogger.Object);

            // Act
            var result = await agent.RunAsync();

            // Assert
            Assert.Equal("test-agent", result.Action);
            Assert.Equal("completed", result.Reason);
            Assert.True(result.Metadata.ContainsKey("elapsed_ms"));
            Assert.IsType<double>(result.Metadata["elapsed_ms"]);
        }

        [Fact]
        public async Task RunAsync_WithCancellation_ReturnsCancelledResult()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<TestAgent>>();
            var agent = new TestAgent(mockLogger.Object);
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act
            var result = await agent.RunAsync(cts.Token);

            // Assert
            Assert.Equal("test-agent", result.Action);
            Assert.Equal("cancelled", result.Reason);
        }

        [Fact]
        public async Task RunAsync_WithException_ReturnsFailedResult()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<FailingAgent>>();
            var agent = new FailingAgent(mockLogger.Object);

            // Act
            var result = await agent.RunAsync();

            // Assert
            Assert.Equal("failing-agent", result.Action);
            Assert.Equal("failed", result.Reason);
            Assert.True(result.Metadata.ContainsKey("error"));
        }

        [Fact]
        public void Constructor_WithNullName_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => new TestAgent(null!, Mock.Of<ILogger<TestAgent>>()));
        }

        private class TestAgent : BaseAgent
        {
            public TestAgent(ILogger logger) : base("test-agent") { }

            protected override async Task<(string reason, IDictionary<string, object> metadata)> LoopAsync(CancellationToken cancellationToken)
            {
                await Task.Delay(10, cancellationToken);
                return ("completed", new Dictionary<string, object>());
            }
        }

        private class FailingAgent : BaseAgent
        {
            public FailingAgent(ILogger logger) : base("failing-agent") { }

            protected override async Task<(string reason, IDictionary<string, object> metadata)> LoopAsync(CancellationToken cancellationToken)
            {
                await Task.Delay(10, cancellationToken);
                throw new InvalidOperationException("Test exception");
            }
        }
    }
}
