using System;
using System.Collections.Generic;
using System.Linq;

namespace UbntSecPilot.Domain.Entities
{
    /// <summary>
    /// Base entity class with common properties
    /// </summary>
    public abstract class BaseEntity
    {
        public string Id { get; protected set; } = Guid.NewGuid().ToString();
        public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; protected set; }

        protected void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Domain exceptions
    /// </summary>
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message) { }
        public DomainException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Event status enumeration
    /// </summary>
    public enum EventStatus
    {
        New,
        Processed,
        Failed
    }

    /// <summary>
    /// Event priority enumeration
    /// </summary>
    public enum EventPriority
    {
        Low,
        Medium,
        High
    }

    /// <summary>
    /// Finding status enumeration
    /// </summary>
    public enum FindingStatus
    {
        Open,
        Escalated,
        Closed
    }

    /// <summary>
    /// Decision status enumeration
    /// </summary>
    public enum DecisionStatus
    {
        Pending,
        Executed,
        Failed
    }

    /// <summary>
    /// Analysis status enumeration
    /// </summary>
    public enum AnalysisStatus
    {
        Pending,
        InProgress,
        Completed,
        Failed
    }
}
