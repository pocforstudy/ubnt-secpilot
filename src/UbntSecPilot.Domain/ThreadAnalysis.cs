using System;
using System.Collections.Generic;
using UbntSecPilot.Domain.ValueObjects;

namespace UbntSecPilot.Domain.Models
{
    /// <summary>
    /// Summary describing whether a thread likely contains IoCs.
    /// </summary>
    public class ThreadAnalysis
    {
        public string Id { get; }
        public string ThreadId { get; }
        public bool IsIoc { get; }
        public string Severity { get; }
        public string Reason { get; }
        public List<string> Indicators { get; }
        public Dictionary<string, object> Metadata { get; }
        public DateTime CreatedAt { get; }
        public DateTime? UpdatedAt { get; private set; }
        public AnalysisStatus Status { get; private set; }

        public ThreadAnalysis(string id, bool isIoc, string severity, string reason, List<string> indicators = null, Dictionary<string, object> metadata = null)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            ThreadId = id; // ThreadId is the same as Id for this implementation
            IsIoc = isIoc;
            Severity = severity ?? throw new ArgumentNullException(nameof(severity));
            Reason = reason ?? throw new ArgumentNullException(nameof(reason));
            Indicators = indicators ?? new List<string>();
            Metadata = metadata ?? new Dictionary<string, object>();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = null;
            Status = AnalysisStatus.Completed;
        }

        public void MarkAsInProgress()
        {
            Status = AnalysisStatus.InProgress;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsCompleted()
        {
            Status = AnalysisStatus.Completed;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsFailed(string reason)
        {
            Status = AnalysisStatus.Failed;
            Metadata["failure_reason"] = reason;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
