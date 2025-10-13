using System;
using System.Collections.Generic;
using Orleans;

namespace UbntSecPilot.Agents
{
    [GenerateSerializer]
    public sealed class AgentResult
    {
        [Id(0)]
        public string Action { get; init; } = string.Empty;

        [Id(1)]
        public string Reason { get; init; } = string.Empty;

        [Id(2)]
        public IReadOnlyDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>();

        [Id(3)]
        public DateTime CreatedAt { get; init; }

        public AgentResult(string action, string reason, IReadOnlyDictionary<string, object> metadata)
        {
            Action = action ?? throw new ArgumentNullException(nameof(action));
            Reason = reason ?? throw new ArgumentNullException(nameof(reason));
            Metadata = metadata ?? new Dictionary<string, object>();
            CreatedAt = DateTime.UtcNow;
        }

        // Parameterless constructor required for Orleans code generation
        private AgentResult()
        {
        }
    }
}
