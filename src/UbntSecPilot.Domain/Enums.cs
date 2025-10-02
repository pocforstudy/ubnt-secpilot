using System;
using System.Collections.Generic;

namespace UbntSecPilot.Domain.ValueObjects
{
    /// <summary>
    /// Firewall action enumeration
    /// </summary>
    public enum FirewallAction
    {
        Accept,
        Drop,
        Reject
    }

    /// <summary>
    /// Network protocol enumeration
    /// </summary>
    public enum NetworkProtocol
    {
        Tcp,
        Udp,
        Icmp,
        All
    }

    /// <summary>
    /// Event status enumeration
    /// </summary>
    public enum EventStatus
    {
        New,
        Processing,
        Processed,
        Failed
    }

    /// <summary>
    /// Threat finding status enumeration
    /// </summary>
    public enum FindingStatus
    {
        Open,
        InProgress,
        Resolved,
        Closed
    }

    /// <summary>
    /// Agent decision status enumeration
    /// </summary>
    public enum DecisionStatus
    {
        Pending,
        Executed,
        Failed,
        Cancelled
    }

    /// <summary>
    /// Thread analysis status enumeration
    /// </summary>
    public enum AnalysisStatus
    {
        Pending,
        InProgress,
        Completed,
        Failed
    }
}
