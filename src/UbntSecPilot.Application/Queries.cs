using MediatR;
using UbntSecPilot.Domain.Models;
using UbntSecPilot.Domain.Services;
using UbntSecPilot.Domain.Entities;

namespace UbntSecPilot.Application.Queries
{
    /// <summary>
    /// Query to get all network events
    /// </summary>
    public class GetAllEventsQuery : IRequest<IEnumerable<NetworkEvent>>
    {
        public int Limit { get; set; } = 100;
        public string Source { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }

    /// <summary>
    /// Query to get a specific network event by ID
    /// </summary>
    public class GetEventByIdQuery : IRequest<NetworkEvent>
    {
        public string EventId { get; set; }

        public GetEventByIdQuery(string eventId)
        {
            EventId = eventId ?? throw new ArgumentNullException(nameof(eventId));
        }
    }

    /// <summary>
    /// Query to get all threat findings
    /// </summary>
    public class GetAllFindingsQuery : IRequest<IEnumerable<ThreatFinding>>
    {
        public int Limit { get; set; } = 100;
        public string Severity { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }

    /// <summary>
    /// Query to get a specific threat finding by ID
    /// </summary>
    public class GetFindingByIdQuery : IRequest<ThreatFinding>
    {
        public string FindingId { get; set; }

        public GetFindingByIdQuery(string findingId)
        {
            FindingId = findingId ?? throw new ArgumentNullException(nameof(findingId));
        }
    }

    /// <summary>
    /// Query to get all agent decisions
    /// </summary>
    public class GetAllDecisionsQuery : IRequest<IEnumerable<AgentDecision>>
    {
        public int Limit { get; set; } = 100;
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }

    /// <summary>
    /// Query to get a specific agent decision by ID
    /// </summary>
    public class GetDecisionByIdQuery : IRequest<AgentDecision>
    {
        public string DecisionId { get; set; }

        public GetDecisionByIdQuery(string decisionId)
        {
            DecisionId = decisionId ?? throw new ArgumentNullException(nameof(decisionId));
        }
    }

    /// <summary>
    /// Query to get all thread analyses
    /// </summary>
    public class GetAllThreadAnalysesQuery : IRequest<IEnumerable<ThreadAnalysis>>
    {
        public int Limit { get; set; } = 100;
    }

    /// <summary>
    /// Query to get a specific thread analysis by ID
    /// </summary>
    public class GetThreadAnalysisByIdQuery : IRequest<ThreadAnalysis>
    {
        public string AnalysisId { get; set; }

        public GetThreadAnalysisByIdQuery(string analysisId)
        {
            AnalysisId = analysisId ?? throw new ArgumentNullException(nameof(analysisId));
        }
    }

    /// <summary>
    /// Query to get system health status
    /// </summary>
    public class GetSystemHealthQuery : IRequest<Dictionary<string, object>>
    {
        public bool IncludeDetails { get; set; } = false;
    }

    /// <summary>
    /// Query to get dashboard statistics
    /// </summary>
    public class GetDashboardStatsQuery : IRequest<Dictionary<string, object>>
    {
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }

    /// <summary>
    /// Query to get firewall rules
    /// </summary>
    public class GetFirewallRulesQuery : IRequest<IEnumerable<Dictionary<string, object>>>
    {
        public string Ruleset { get; set; }
        public string Action { get; set; }
        public string SearchTerm { get; set; }
    }

    /// <summary>
    /// Query to get VirusTotal usage statistics
    /// </summary>
    public class GetVirusTotalUsageQuery : IRequest<Dictionary<string, object>>
    {
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}
