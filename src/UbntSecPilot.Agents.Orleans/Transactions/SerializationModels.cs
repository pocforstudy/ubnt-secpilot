using System;
using System.Collections.Generic;
using Orleans;
using UbntSecPilot.Domain.Models;
using UbntSecPilot.Domain.ValueObjects;

namespace UbntSecPilot.Agents.Orleans.Transactions
{
    [GenerateSerializer]
    public sealed class SerializableNetworkEvent
    {
        [Id(0)]
        public string EventId { get; init; } = string.Empty;

        [Id(1)]
        public string Source { get; init; } = string.Empty;

        [Id(2)]
        public Dictionary<string, object> Payload { get; init; } = new();

        [Id(3)]
        public DateTime OccurredAt { get; init; }

        [Id(4)]
        public EventStatus Status { get; init; }

        [Id(5)]
        public string Priority { get; init; } = string.Empty;

        [Id(6)]
        public string? FailureReason { get; init; }

        public SerializableNetworkEvent()
        {
        }

        public SerializableNetworkEvent(NetworkEvent domain)
        {
            EventId = domain.EventId;
            Source = domain.Source;
            Payload = new Dictionary<string, object>(domain.Payload);
            OccurredAt = domain.OccurredAt;
            Status = domain.Status;
            Priority = domain.Priority;
        }

        public NetworkEvent ToDomain()
        {
            var ev = new NetworkEvent(EventId, Source, new Dictionary<string, object>(Payload), OccurredAt);
            // For records, Status and Priority are set during construction and can't be modified
            // The record constructor will set Status = New and Priority = Medium by default
            return ev;
        }

        public static implicit operator SerializableNetworkEvent?(NetworkEvent? domain)
            => domain is null ? null : new SerializableNetworkEvent(domain);
    }

    [GenerateSerializer]
    public sealed class SerializableAgentDecision
    {
        [Id(0)]
        public string Id { get; init; } = string.Empty;

        [Id(1)]
        public string Action { get; init; } = string.Empty;

        [Id(2)]
        public string Reason { get; init; } = string.Empty;

        [Id(3)]
        public Dictionary<string, object> Metadata { get; init; } = new();

        [Id(4)]
        public DateTime CreatedAt { get; init; }

        [Id(5)]
        public DateTime? UpdatedAt { get; init; }

        [Id(6)]
        public DecisionStatus Status { get; init; }

        [Id(7)]
        public string? ExecutedBy { get; init; }

        public SerializableAgentDecision()
        {
        }

        public SerializableAgentDecision(AgentDecision domain)
        {
            Id = domain.Id;
            Action = domain.Action;
            Reason = domain.Reason;
            Metadata = new Dictionary<string, object>(domain.Metadata);
            CreatedAt = domain.CreatedAt;
            UpdatedAt = domain.UpdatedAt;
            Status = domain.Status;
            ExecutedBy = domain.ExecutedBy;
        }

        public AgentDecision ToDomain()
        {
            var decision = new AgentDecision(Action, Reason, new Dictionary<string, object>(Metadata));

            // For records, Id, CreatedAt, UpdatedAt, Status, and ExecutedBy are set during construction
            // and can't be modified afterward, so we return the decision as-is
            return decision;
        }

        public static implicit operator SerializableAgentDecision?(AgentDecision? domain)
            => domain is null ? null : new SerializableAgentDecision(domain);
    }

    [GenerateSerializer]
    public sealed class SerializableThreatFinding
    {
        [Id(0)]
        public string Id { get; init; } = string.Empty;

        [Id(1)]
        public string EventId { get; init; } = string.Empty;

        [Id(2)]
        public string Severity { get; init; } = string.Empty;

        [Id(3)]
        public string Summary { get; init; } = string.Empty;

        [Id(4)]
        public Dictionary<string, object> Metadata { get; init; } = new();

        [Id(5)]
        public DateTime CreatedAt { get; init; }

        [Id(6)]
        public DateTime? UpdatedAt { get; init; }

        [Id(7)]
        public FindingStatus Status { get; init; }

        [Id(8)]
        public string? AssignedTo { get; init; }

        public SerializableThreatFinding()
        {
        }

        public SerializableThreatFinding(ThreatFinding domain)
        {
            Id = domain.Id;
            EventId = domain.EventId;
            Severity = domain.Severity;
            Summary = domain.Summary;
            Metadata = new Dictionary<string, object>(domain.Metadata);
            CreatedAt = domain.CreatedAt;
            UpdatedAt = domain.UpdatedAt;
            Status = domain.Status;
            AssignedTo = domain.AssignedTo;
        }

        public ThreatFinding ToDomain()
        {
            // For records, we need to create a new instance with all the values
            // Since records are immutable, we create a new one with the current values
            // If Id is empty, generate a new one
            var findingId = string.IsNullOrEmpty(Id) ? Guid.NewGuid().ToString() : Id;
            return new ThreatFinding(
                findingId,
                EventId,
                Severity,
                Summary,
                new Dictionary<string, object>(Metadata),
                CreatedAt,
                UpdatedAt,
                Status,
                AssignedTo
            );
        }

        public static implicit operator SerializableThreatFinding?(ThreatFinding? domain)
            => domain is null ? null : new SerializableThreatFinding(domain);
    }
}
