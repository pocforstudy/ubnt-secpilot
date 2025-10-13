using System;
using System.Collections.Generic;
using UbntSecPilot.Domain.ValueObjects;

namespace UbntSecPilot.Domain.Models
{
    /// <summary>
    /// Threat finding with severity and metadata
    /// </summary>
    public record ThreatFinding(
        string Id,
        string EventId,
        string Severity,
        string Summary,
        Dictionary<string, object> Metadata,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        FindingStatus Status,
        string? AssignedTo
    )
    {
        public ThreatFinding(string eventId, string severity, string summary, Dictionary<string, object>? metadata = null)
            : this(
                Guid.NewGuid().ToString(),
                eventId ?? throw new ArgumentNullException(nameof(eventId)),
                severity ?? throw new ArgumentNullException(nameof(severity)),
                summary ?? throw new ArgumentNullException(nameof(summary)),
                metadata ?? new Dictionary<string, object>(),
                DateTime.UtcNow,
                null,
                FindingStatus.Open,
                null
            )
        {
        }

        public ThreatFinding AssignTo(string user)
        {
            // Records are immutable, so we return a new instance
            return this with
            {
                AssignedTo = user,
                Status = FindingStatus.InProgress,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public ThreatFinding MarkAsResolved()
        {
            return this with
            {
                Status = FindingStatus.Resolved,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public ThreatFinding MarkAsClosed()
        {
            return this with
            {
                Status = FindingStatus.Closed,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }
}
