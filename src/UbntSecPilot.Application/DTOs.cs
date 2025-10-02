using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UbntSecPilot.Application.DTOs
{
    /// <summary>
    /// Data Transfer Object for Network Events
    /// </summary>
    public class NetworkEventDto
    {
        /// <summary>
        /// Unique identifier for the network event
        /// </summary>
        /// <example>evt_64f1e8b2-3c4d-5e6f-7g8h-9i0j1k2l3m4n</example>
        public string EventId { get; set; }

        /// <summary>
        /// Source of the network event (e.g., udm-pro, firewall, ids)
        /// </summary>
        /// <example>udm-pro</example>
        public string Source { get; set; }

        /// <summary>
        /// Event payload containing detailed information
        /// </summary>
        public Dictionary<string, object> Payload { get; set; }

        /// <summary>
        /// Timestamp when the event occurred
        /// </summary>
        /// <example>2024-01-15T10:30:00Z</example>
        public DateTime OccurredAt { get; set; }

        /// <summary>
        /// Current processing status of the event
        /// </summary>
        /// <example>processed</example>
        public string Status { get; set; }

        /// <summary>
        /// Priority level of the event
        /// </summary>
        /// <example>high</example>
        public string Priority { get; set; }

        /// <summary>
        /// Short summary of the event for display purposes
        /// </summary>
        /// <example>udm-pro: suspicious_ip @ 2024-01-15T10:30:00Z</example>
        public string ShortSummary { get; set; }

        /// <summary>
        /// Timestamp when the event was created in the system
        /// </summary>
        /// <example>2024-01-15T10:30:00Z</example>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp when the event was last updated
        /// </summary>
        /// <example>2024-01-15T10:30:00Z</example>
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// Data Transfer Object for Threat Findings
    /// </summary>
    public class ThreatFindingDto
    {
        /// <summary>
        /// Unique identifier for the threat finding
        /// </summary>
        /// <example>fnd_64f1e8b2-3c4d-5e6f-7g8h-9i0j1k2l3m4n</example>
        public string Id { get; set; }

        /// <summary>
        /// ID of the network event that triggered this finding
        /// </summary>
        /// <example>evt_64f1e8b2-3c4d-5e6f-7g8h-9i0j1k2l3m4n</example>
        public string EventId { get; set; }

        /// <summary>
        /// Severity level of the threat
        /// </summary>
        /// <example>high</example>
        public string Severity { get; set; }

        /// <summary>
        /// Detailed summary of the threat finding
        /// </summary>
        /// <example>Suspicious network activity detected from IP 192.168.1.100</example>
        public string Summary { get; set; }

        /// <summary>
        /// Additional metadata associated with the finding
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; }

        /// <summary>
        /// Current status of the finding
        /// </summary>
        /// <example>open</example>
        public string Status { get; set; }

        /// <summary>
        /// User assigned to handle this finding
        /// </summary>
        /// <example>analyst@company.com</example>
        public string AssignedTo { get; set; }

        /// <summary>
        /// Timestamp when the finding was created
        /// </summary>
        /// <example>2024-01-15T10:30:00Z</example>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp when the finding was last updated
        /// </summary>
        /// <example>2024-01-15T10:30:00Z</example>
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// Data Transfer Object for Agent Decisions
    /// </summary>
    public class AgentDecisionDto
    {
        /// <summary>
        /// Unique identifier for the agent decision
        /// </summary>
        /// <example>dec_64f1e8b2-3c4d-5e6f-7g8h-9i0j1k2l3m4n</example>
        public string Id { get; set; }

        /// <summary>
        /// Action taken by the agent
        /// </summary>
        /// <example>block_ip</example>
        public string Action { get; set; }

        /// <summary>
        /// Reason for the agent's decision
        /// </summary>
        /// <example>Blocking suspicious IP address due to multiple failed login attempts</example>
        public string Reason { get; set; }

        /// <summary>
        /// Additional metadata for the decision
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; }

        /// <summary>
        /// Current status of the decision
        /// </summary>
        /// <example>executed</example>
        public string Status { get; set; }

        /// <summary>
        /// User who executed the decision
        /// </summary>
        /// <example>admin@company.com</example>
        public string ExecutedBy { get; set; }

        /// <summary>
        /// Timestamp when the decision was created
        /// </summary>
        /// <example>2024-01-15T10:30:00Z</example>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp when the decision was last updated
        /// </summary>
        /// <example>2024-01-15T10:30:00Z</example>
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// Data Transfer Object for Thread Analysis
    /// </summary>
    public class ThreadAnalysisDto
    {
        /// <summary>
        /// Unique identifier for the thread analysis
        /// </summary>
        /// <example>ana_64f1e8b2-3c4d-5e6f-7g8h-9i0j1k2l3m4n</example>
        public string Id { get; set; }

        /// <summary>
        /// ID of the thread being analyzed
        /// </summary>
        /// <example>thread-001</example>
        public string ThreadId { get; set; }

        /// <summary>
        /// Whether the thread contains indicators of compromise
        /// </summary>
        /// <example>true</example>
        public bool IsIoc { get; set; }

        /// <summary>
        /// Severity level of the analysis
        /// </summary>
        /// <example>high</example>
        public string Severity { get; set; }

        /// <summary>
        /// Analysis result explanation
        /// </summary>
        /// <example>Potential indicators of compromise detected in thread content</example>
        public string Reason { get; set; }

        /// <summary>
        /// List of indicators found in the thread
        /// </summary>
        public List<string> Indicators { get; set; }

        /// <summary>
        /// Additional metadata for the analysis
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; }

        /// <summary>
        /// Current status of the analysis
        /// </summary>
        /// <example>completed</example>
        public string Status { get; set; }

        /// <summary>
        /// Timestamp when the analysis was created
        /// </summary>
        /// <example>2024-01-15T10:30:00Z</example>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp when the analysis was last updated
        /// </summary>
        /// <example>2024-01-15T10:30:00Z</example>
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// Data Transfer Object for Thread Messages
    /// </summary>
    public class ThreadMessageDto
    {
        /// <summary>
        /// Content of the message
        /// </summary>
        /// <example>Investigating possible beacon to https://c2.example.com</example>
        [Required]
        [StringLength(5000, MinimumLength = 1)]
        public string Content { get; set; }

        /// <summary>
        /// Author of the message
        /// </summary>
        /// <example>analyst</example>
        public string Author { get; set; }

        /// <summary>
        /// Timestamp when the message was created
        /// </summary>
        /// <example>2024-01-15T10:30:00Z</example>
        public DateTime? CreatedAt { get; set; }
    }

    /// <summary>
    /// Data Transfer Object for Firewall Rules
    /// </summary>
    public class FirewallRuleDto
    {
        /// <summary>
        /// Unique identifier for the firewall rule
        /// </summary>
        /// <example>rule_64f1e8b2-3c4d-5e6f-7g8h-9i0j1k2l3m4n</example>
        public string Id { get; set; }

        /// <summary>
        /// Name of the firewall rule
        /// </summary>
        /// <example>Block Suspicious IPs</example>
        public string Name { get; set; }

        /// <summary>
        /// Ruleset the rule belongs to
        /// </summary>
        /// <example>LAN_IN</example>
        public string Ruleset { get; set; }

        /// <summary>
        /// Action to take (accept, drop, reject)
        /// </summary>
        /// <example>drop</example>
        public string Action { get; set; }

        /// <summary>
        /// Network protocol
        /// </summary>
        /// <example>tcp</example>
        public string Protocol { get; set; }

        /// <summary>
        /// Destination IP address or subnet
        /// </summary>
        /// <example>192.168.1.0/24</example>
        public string DstAddress { get; set; }

        /// <summary>
        /// Destination port number
        /// </summary>
        /// <example>80</example>
        public string DstPort { get; set; }

        /// <summary>
        /// Description of the rule
        /// </summary>
        /// <example>Block HTTP traffic to suspicious destinations</example>
        public string Description { get; set; }

        /// <summary>
        /// Whether the rule is enabled
        /// </summary>
        /// <example>true</example>
        public bool Enabled { get; set; }
    }

    /// <summary>
    /// Data Transfer Object for System Health
    /// </summary>
    public class SystemHealthDto
    {
        /// <summary>
        /// Overall system status
        /// </summary>
        /// <example>healthy</example>
        public string Status { get; set; }

        /// <summary>
        /// Timestamp of the health check
        /// </summary>
        /// <example>2024-01-15T10:30:00Z</example>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Status of individual services
        /// </summary>
        public Dictionary<string, string> Services { get; set; }

        /// <summary>
        /// Detailed health information
        /// </summary>
        public Dictionary<string, object> Details { get; set; }
    }

    /// <summary>
    /// Data Transfer Object for Dashboard Statistics
    /// </summary>
    public class DashboardStatsDto
    {
        /// <summary>
        /// Total number of events stored
        /// </summary>
        /// <example>1250</example>
        public int TotalEvents { get; set; }

        /// <summary>
        /// Total number of findings generated
        /// </summary>
        /// <example>45</example>
        public int TotalFindings { get; set; }

        /// <summary>
        /// Total number of decisions made
        /// </summary>
        /// <example>23</example>
        public int TotalDecisions { get; set; }

        /// <summary>
        /// Number of critical findings
        /// </summary>
        /// <example>3</example>
        public int CriticalFindings { get; set; }

        /// <summary>
        /// Number of high priority findings
        /// </summary>
        /// <example>12</example>
        public int HighFindings { get; set; }

        /// <summary>
        /// Number of medium priority findings
        /// </summary>
        /// <example>18</example>
        public int MediumFindings { get; set; }

        /// <summary>
        /// Number of low priority findings
        /// </summary>
        /// <example>12</example>
        public int LowFindings { get; set; }

        /// <summary>
        /// Events grouped by source
        /// </summary>
        public Dictionary<string, int> EventsBySource { get; set; }

        /// <summary>
        /// Findings grouped by severity
        /// </summary>
        public Dictionary<string, int> FindingsBySeverity { get; set; }

        /// <summary>
        /// Time range for the statistics
        /// </summary>
        public object TimeRange { get; set; }
    }

    /// <summary>
    /// Data Transfer Object for VirusTotal Usage
    /// </summary>
    public class VirusTotalUsageDto
    {
        /// <summary>
        /// Total number of lookups performed
        /// </summary>
        /// <example>1250</example>
        public int TotalLookups { get; set; }

        /// <summary>
        /// Number of lookups requested
        /// </summary>
        /// <example>1200</example>
        public int LookupsRequested { get; set; }

        /// <summary>
        /// Number of lookups skipped due to quota limits
        /// </summary>
        /// <example>50</example>
        public int SkippedDueToQuota { get; set; }

        /// <summary>
        /// Latest quota information
        /// </summary>
        public Dictionary<string, object> LatestQuota { get; set; }

        /// <summary>
        /// Timeline of VirusTotal operations
        /// </summary>
        public List<Dictionary<string, object>> Timeline { get; set; }

        /// <summary>
        /// List of blocked items
        /// </summary>
        public List<Dictionary<string, object>> BlockedItems { get; set; }
    }

    /// <summary>
    /// Data Transfer Object for API Response wrapper
    /// </summary>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Indicates if the operation was successful
        /// </summary>
        /// <example>true</example>
        public bool Success { get; set; }

        /// <summary>
        /// Response data payload
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// Response message
        /// </summary>
        /// <example>Operation completed successfully</example>
        public string Message { get; set; }

        /// <summary>
        /// List of error messages if the operation failed
        /// </summary>
        public List<string> Errors { get; set; }

        public ApiResponse(T data, string? message = null)
        {
            Success = true;
            Data = data;
            Message = message;
            Errors = new List<string>();
        }

        public ApiResponse(string message, List<string> errors)
        {
            Success = false;
            Data = default;
            Message = message;
            Errors = errors ?? new List<string>();
        }
    }

    /// <summary>
    /// Data Transfer Object for paginated results
    /// </summary>
    public class PaginatedResult<T>
    {
        /// <summary>
        /// Data items for the current page
        /// </summary>
        public IEnumerable<T> Items { get; set; }

        /// <summary>
        /// Total number of items across all pages
        /// </summary>
        /// <example>100</example>
        public int TotalCount { get; set; }

        /// <summary>
        /// Current page number (1-based)
        /// </summary>
        /// <example>1</example>
        public int PageNumber { get; set; }

        /// <summary>
        /// Number of items per page
        /// </summary>
        /// <example>20</example>
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of pages
        /// </summary>
        /// <example>5</example>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        public PaginatedResult(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }
}
