using System;
using System.Collections.Generic;

namespace UbntSecPilot.Domain.Models
{
    /// <summary>
    /// Reputation verdict from external services like VirusTotal.
    /// </summary>
    public class ReputationVerdict
    {
        public string Id { get; }
        public string Indicator { get; }
        public string Service { get; }
        public string Verdict { get; }
        public double Confidence { get; }
        public Dictionary<string, object> Details { get; }
        public Dictionary<string, object> Metadata { get; }
        public DateTime CheckedAt { get; }
        public DateTime CreatedAt { get; }
        public DateTime? UpdatedAt { get; }

        public ReputationVerdict(string indicator, string service, string verdict, double confidence, Dictionary<string, object> details = null, Dictionary<string, object> metadata = null)
        {
            Id = Guid.NewGuid().ToString();
            Indicator = indicator ?? throw new ArgumentNullException(nameof(indicator));
            Service = service ?? throw new ArgumentNullException(nameof(service));
            Verdict = verdict ?? throw new ArgumentNullException(nameof(verdict));

            if (confidence < 0.0 || confidence > 1.0)
                throw new ArgumentOutOfRangeException(nameof(confidence), "Confidence must be between 0.0 and 1.0");

            Confidence = confidence;
            Details = details ?? new Dictionary<string, object>();
            Metadata = metadata ?? new Dictionary<string, object>();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = null;
        }
    }
}
