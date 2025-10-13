using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;

namespace UbntSecPilot.Agents.Orleans
{
    public sealed class AgentOrchestratorGrain : Grain, IAgentOrchestrator
    {
        private readonly ILogger<AgentOrchestratorGrain> _logger;
        private readonly IGrainFactory _grainFactory;

        private static readonly IReadOnlyList<string> _availableAgents = new[]
        {
            "threat-enrichment"
        };

        public AgentOrchestratorGrain(
            ILogger<AgentOrchestratorGrain> logger,
            IGrainFactory grainFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _grainFactory = grainFactory ?? throw new ArgumentNullException(nameof(grainFactory));
        }

        public Task<IReadOnlyList<string>> GetAvailableAgentsAsync() => Task.FromResult(_availableAgents);

        public async Task<AgentStatus> GetAgentStatusAsync(string agentName)
        {
            var agentGrain = _grainFactory.GetGrain<IAgentGrain>(agentName);
            var isRunning = await agentGrain.IsRunning().ConfigureAwait(false);
            var status = await agentGrain.GetStatus().ConfigureAwait(false);

            return new AgentStatus(agentName, status, isRunning);
        }

        public async Task<AgentResult> RunAgentAsync(string agentName, CancellationToken cancellationToken = default)
        {
            if (!_availableAgents.Contains(agentName))
            {
                throw new KeyNotFoundException($"Agent '{agentName}' is not registered");
            }

            _logger.LogInformation("Running agent {AgentName}", agentName);

            var agentGrain = _grainFactory.GetGrain<IAgentGrain>(agentName);
            return await agentGrain.RunAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
