using System;
using System.Collections.Generic;
using UbntSecPilot.Domain.ValueObjects;

namespace UbntSecPilot.Domain.Models
{
    /// <summary>
    /// Agent decision with action and metadata
    /// </summary>
    public record AgentDecision(
        string Id,
        string Action,
        string Reason,
        Dictionary<string, object> Metadata,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        DecisionStatus Status,
        string? ExecutedBy
    )
    {
        public AgentDecision(string action, string reason, Dictionary<string, object>? metadata = null)
            : this(
                Guid.NewGuid().ToString(),
                action ?? throw new ArgumentNullException(nameof(action)),
                reason ?? throw new ArgumentNullException(nameof(reason)),
                metadata ?? new Dictionary<string, object>(),
                DateTime.UtcNow,
                null,
                DecisionStatus.Pending,
                null
            )
        {
        }

        public AgentDecision MarkAsExecuted(string executedBy)
        {
            return this with
            {
                Status = DecisionStatus.Executed,
                ExecutedBy = executedBy,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public AgentDecision MarkAsFailed(string reason)
        {
            return this with
            {
                Status = DecisionStatus.Failed,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public AgentDecision Cancel()
        {
            return this with
            {
                Status = DecisionStatus.Cancelled,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }
}
