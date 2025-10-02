using System;
using System.Collections.Generic;
using UbntSecPilot.Domain.ValueObjects;

namespace UbntSecPilot.Domain.Models
{
    /// <summary>
    /// Agent decision with action and metadata
    /// </summary>
    public class AgentDecision
    {
        public string Id { get; }
        public string Action { get; }
        public string Reason { get; }
        public Dictionary<string, object> Metadata { get; }
        public DateTime CreatedAt { get; }
        public DateTime? UpdatedAt { get; private set; }
        public DecisionStatus Status { get; private set; }
        public string ExecutedBy { get; private set; }

        public AgentDecision(string action, string reason, Dictionary<string, object> metadata = null)
        {
            Id = Guid.NewGuid().ToString();
            Action = action ?? throw new ArgumentNullException(nameof(action));
            Reason = reason ?? throw new ArgumentNullException(nameof(reason));
            Metadata = metadata ?? new Dictionary<string, object>();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = null;
            Status = DecisionStatus.Pending;
            ExecutedBy = null;
        }

        public void MarkAsExecuted(string executedBy)
        {
            Status = DecisionStatus.Executed;
            ExecutedBy = executedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsFailed(string reason)
        {
            Status = DecisionStatus.Failed;
            Metadata["failure_reason"] = reason;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Cancel()
        {
            Status = DecisionStatus.Cancelled;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
