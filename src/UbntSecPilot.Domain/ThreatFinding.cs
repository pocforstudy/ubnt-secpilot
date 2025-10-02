using System;
using System.Collections.Generic;
using UbntSecPilot.Domain.ValueObjects;

namespace UbntSecPilot.Domain.Models
{
    /// <summary>
    /// Threat finding with severity and metadata
    /// </summary>
    public class ThreatFinding
    {
        public string Id { get; }
        public string EventId { get; }
        public string Severity { get; }
        public string Summary { get; }
        public Dictionary<string, object> Metadata { get; }
        public DateTime CreatedAt { get; }
        public DateTime? UpdatedAt { get; private set; }
        public FindingStatus Status { get; private set; }
        public string AssignedTo { get; private set; }

        public ThreatFinding(string eventId, string severity, string summary, Dictionary<string, object> metadata = null)
        {
            Id = Guid.NewGuid().ToString();
            EventId = eventId ?? throw new ArgumentNullException(nameof(eventId));
            Severity = severity ?? throw new ArgumentNullException(nameof(severity));
            Summary = summary ?? throw new ArgumentNullException(nameof(summary));
            Metadata = metadata ?? new Dictionary<string, object>();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = null;
            Status = FindingStatus.Open;
            AssignedTo = null;
        }

        public void AssignTo(string user)
        {
            AssignedTo = user;
            Status = FindingStatus.InProgress;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsResolved()
        {
            Status = FindingStatus.Resolved;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsClosed()
        {
            Status = FindingStatus.Closed;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
