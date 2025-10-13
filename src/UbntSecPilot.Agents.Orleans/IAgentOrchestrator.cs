using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using Orleans.Serialization;

namespace UbntSecPilot.Agents.Orleans
{
    public interface IAgentOrchestrator : IGrainWithStringKey
    {
        Task<AgentResult> RunAgentAsync(string agentName, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<string>> GetAvailableAgentsAsync();
        Task<AgentStatus> GetAgentStatusAsync(string agentName);
    }

    [GenerateSerializer]
    public sealed class AgentStatus
    {
        [Id(0)]
        public string AgentName { get; init; } = string.Empty;

        [Id(1)]
        public string Status { get; init; } = string.Empty;

        [Id(2)]
        public bool IsRunning { get; init; }

        [Id(3)]
        public DateTime LastUpdated { get; init; }

        public AgentStatus(string agentName, string status, bool isRunning)
        {
            AgentName = agentName ?? throw new ArgumentNullException(nameof(agentName));
            Status = status ?? throw new ArgumentNullException(nameof(status));
            IsRunning = isRunning;
            LastUpdated = DateTime.UtcNow;
        }

        private AgentStatus()
        {
        }
    }
}
