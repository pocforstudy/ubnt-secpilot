using System;
using System.Collections.Generic;
using UbntSecPilot.Domain.ValueObjects;

namespace UbntSecPilot.Domain.Models
{
    /// <summary>
    /// Immutable network/security event captured from UDM sensors.
    /// </summary>
    public class NetworkEvent
    {
        public string EventId { get; }
        public string Source { get; }
        public Dictionary<string, object> Payload { get; }
        public DateTime OccurredAt { get; }
        public EventStatus Status { get; private set; }
        public string Priority { get; private set; }

        public NetworkEvent(string eventId, string source, Dictionary<string, object> payload, DateTime occurredAt)
        {
            EventId = eventId ?? throw new ArgumentNullException(nameof(eventId));
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Payload = payload ?? new Dictionary<string, object>();
            OccurredAt = occurredAt;
            Status = EventStatus.New;
            Priority = DeterminePriority();
        }

        public void MarkAsProcessed()
        {
            Status = EventStatus.Processed;
        }

        public void MarkAsFailed(string reason)
        {
            Status = EventStatus.Failed;
        }

        private string DeterminePriority()
        {
            // Business logic to determine priority based on payload
            if (Payload.TryGetValue("severity", out var severity) && severity.ToString() == "critical")
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
