using System;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using UbntSecPilot.Agents.Orleans;
using Xunit;

namespace UbntSecPilot.Orleans.Tests
{
    [Collection("OrleansCluster")]
    public class AgentGrainTests : IClassFixture<OrleansTestClusterFixture>
    {
        private readonly OrleansTestClusterFixture _fixture;

        public AgentGrainTests(OrleansTestClusterFixture fixture)
        {
            _fixture = fixture;
        }

        private IAgentGrain Agent(string key) => _fixture.Cluster.GrainFactory.GetGrain<IAgentGrain>(key);

        [Fact]
        public async Task RunAsync_WithNoEvents_LeavesAgentIdle()
        {
            var grain = Agent(Guid.NewGuid().ToString("N"));

            var result = await grain.RunAsync(CancellationToken.None);

            Assert.Equal("threat-enrichment", result.Action);
            Assert.Equal("idle", await grain.GetStatus());
            Assert.False(await grain.IsRunning());
        }

        [Fact]
        public async Task RunAsync_WithCancelledToken_ReturnsCancelled()
        {
            var grain = Agent(Guid.NewGuid().ToString("N"));
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            var result = await grain.RunAsync(cts.Token);

            Assert.Equal("threat-enrichment", result.Action);
            Assert.Equal("idle", await grain.GetStatus());
            Assert.False(await grain.IsRunning());
        }
    }
}
