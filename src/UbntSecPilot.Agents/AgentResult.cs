using System;
using System.Collections.Generic;

namespace UbntSecPilot.Agents
{
    public sealed class AgentResult
    {
        public string Action { get; }
        public string Reason { get; }
        public IReadOnlyDictionary<string, object> Metadata { get; }
        public DateTime CreatedAt { get; }

        public AgentResult(string action, string reason, IReadOnlyDictionary<string, object> metadata)
        {
            Action = action ?? throw new ArgumentNullException(nameof(action));
            Reason = reason ?? throw new ArgumentNullException(nameof(reason));
            Metadata = metadata ?? new Dictionary<string, object>();
            CreatedAt = DateTime.UtcNow;
        }
    }
}
