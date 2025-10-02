using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;

namespace UbntSecPilot.Agents.Orleans
{
    public interface IAgentOrchestrator : IGrainWithStringKey
    {
        Task<AgentResult> RunAgentAsync(string agentName, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<string>> GetAvailableAgentsAsync();
        Task<AgentStatus> GetAgentStatusAsync(string agentName);
    }

    public sealed class AgentStatus
    {
        public string AgentName { get; }
        public string Status { get; }
        public bool IsRunning { get; }
        public DateTime LastUpdated { get; }

        public AgentStatus(string agentName, string status, bool isRunning)
        {
            AgentName = agentName ?? throw new ArgumentNullException(nameof(agentName));
            Status = status ?? throw new ArgumentNullException(nameof(status));
            IsRunning = isRunning;
            LastUpdated = DateTime.UtcNow;
        }
    }
}
