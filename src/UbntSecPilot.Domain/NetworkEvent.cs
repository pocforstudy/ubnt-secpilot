using System;
using System.Collections.Generic;
using UbntSecPilot.Domain.ValueObjects;

namespace UbntSecPilot.Domain.Models
{
    /// <summary>
    /// Immutable network/security event captured from UDM sensors.
    /// </summary>
    public record NetworkEvent(
        string EventId,
        string Source,
        Dictionary<string, object> Payload,
        DateTime OccurredAt,
        EventStatus Status,
        string Priority
    )
    {
        public NetworkEvent(string eventId, string source, Dictionary<string, object> payload, DateTime occurredAt)
            : this(
                eventId ?? throw new ArgumentNullException(nameof(eventId)),
                source ?? throw new ArgumentNullException(nameof(source)),
                payload ?? new Dictionary<string, object>(),
                occurredAt,
                EventStatus.New,
                DeterminePriority(payload ?? new Dictionary<string, object>())
            )
        {
        }

        public NetworkEvent MarkAsProcessed()
        {
            // Records are immutable, so we return a new instance with updated status
            return this with { Status = EventStatus.Processed };
        }

        public NetworkEvent MarkAsFailed(string reason)
        {
            return this with { Status = EventStatus.Failed };
        }

        private static string DeterminePriority(Dictionary<string, object> payload)
        {
            // Business logic to determine priority based on payload
            if (payload.TryGetValue("severity", out var severity) && severity.ToString() == "critical")
                return "High";

            return "Medium";
        }

        /// <summary>
        /// Return a readable summary to log/inspect in the UI.
        /// </summary>
        public string ShortSummary()
        {
            var indicator = Payload.TryGetValue("indicator", out var indicatorValue) ? indicatorValue.ToString() : "unknown";
            return $"{Source}:{indicator} @ {OccurredAt:O}";
        }
    }
}
