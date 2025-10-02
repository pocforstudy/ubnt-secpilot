using MediatR;
using UbntSecPilot.Domain.Models;
using UbntSecPilot.Domain.Services;
using UbntSecPilot.Domain.Entities;
using System.Threading.Tasks;
using UbntSecPilot.Application.Commands;
using UbntSecPilot.Application.Queries;
using UbntSecPilot.Domain.Repositories;
namespace UbntSecPilot.Application.Handlers
{
    /// <summary>
    /// Handler for analyzing network events
    /// </summary>
    public class AnalyzeNetworkEventHandler : IRequestHandler<AnalyzeNetworkEventCommand, ThreatFinding>
    {
        private readonly IThreatAnalysisService _threatAnalysisService;
        private readonly INetworkEventRepository _eventRepository;

        public AnalyzeNetworkEventHandler(IThreatAnalysisService threatAnalysisService, INetworkEventRepository eventRepository)
        {
            _threatAnalysisService = threatAnalysisService ?? throw new ArgumentNullException(nameof(threatAnalysisService));
            _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
        }

        public async Task<ThreatFinding> Handle(AnalyzeNetworkEventCommand request, CancellationToken cancellationToken)
        {
            var networkEvent = new NetworkEvent(request.EventId, request.Source, request.Payload, request.OccurredAt);

            var finding = await _threatAnalysisService.AnalyzeNetworkEventAsync(networkEvent);

            if (finding != null)
            {
                networkEvent.MarkAsProcessed();
                await _eventRepository.UpdateAsync(networkEvent);
            }

            return finding;
        }
    }

    /// <summary>
    /// Handler for running threat enrichment
    /// </summary>
    public class RunThreatEnrichmentHandler : IRequestHandler<RunThreatEnrichmentCommand, AgentDecision>
    {
        private readonly IThreatAnalysisService _threatAnalysisService;

        public RunThreatEnrichmentHandler(IThreatAnalysisService threatAnalysisService)
        {
            _threatAnalysisService = threatAnalysisService ?? throw new ArgumentNullException(nameof(threatAnalysisService));
        }

        public async Task<AgentDecision> Handle(RunThreatEnrichmentCommand request, CancellationToken cancellationToken)
        {
            return await _threatAnalysisService.RunThreatEnrichmentAsync();
        }
    }

    /// <summary>
    /// Handler for analyzing threads
    /// </summary>
    public class AnalyzeThreadHandler : IRequestHandler<AnalyzeThreadCommand, ThreadAnalysis>
    {
        private readonly IThreadAnalysisService _threadAnalysisService;

        public AnalyzeThreadHandler(IThreadAnalysisService threadAnalysisService)
        {
            _threadAnalysisService = threadAnalysisService ?? throw new ArgumentNullException(nameof(threadAnalysisService));
        }

        public async Task<ThreadAnalysis> Handle(AnalyzeThreadCommand request, CancellationToken cancellationToken)
        {
            return await _threadAnalysisService.AnalyzeThreadAsync(request.Messages);
        }
    }

    /// <summary>
    /// Handler for getting all events
    /// </summary>
    public class GetAllEventsHandler : IRequestHandler<GetAllEventsQuery, IEnumerable<NetworkEvent>>
    {
        private readonly INetworkEventRepository _eventRepository;

        public GetAllEventsHandler(INetworkEventRepository eventRepository)
        {
            _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
        }

        public async Task<IEnumerable<NetworkEvent>> Handle(GetAllEventsQuery request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(request.Source))
            {
                return await _eventRepository.GetBySourceAsync(request.Source, request.Limit);
            }

            if (request.StartTime.HasValue && request.EndTime.HasValue)
            {
                return await _eventRepository.GetByTimeRangeAsync(request.StartTime.Value, request.EndTime.Value);
            }

            return await _eventRepository.GetAllAsync(request.Limit);
        }
    }

    /// <summary>
    /// Handler for getting event by ID
    /// </summary>
    public class GetEventByIdHandler : IRequestHandler<GetEventByIdQuery, NetworkEvent>
    {
        private readonly INetworkEventRepository _eventRepository;

        public GetEventByIdHandler(INetworkEventRepository eventRepository)
        {
            _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
        }

        public async Task<NetworkEvent> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
        {
            return await _eventRepository.GetByIdAsync(request.EventId);
        }
    }

    /// <summary>
    /// Handler for getting all findings
    /// </summary>
    public class GetAllFindingsHandler : IRequestHandler<GetAllFindingsQuery, IEnumerable<ThreatFinding>>
    {
        private readonly IThreatFindingRepository _findingRepository;

        public GetAllFindingsHandler(IThreatFindingRepository findingRepository)
        {
            _findingRepository = findingRepository ?? throw new ArgumentNullException(nameof(findingRepository));
        }

        public async Task<IEnumerable<ThreatFinding>> Handle(GetAllFindingsQuery request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(request.Severity))
            {
                return await _findingRepository.GetBySeverityAsync(request.Severity, request.Limit);
            }

            return await _findingRepository.GetAllAsync(request.Limit);
        }
    }

    /// <summary>
    /// Handler for getting finding by ID
    /// </summary>
    public class GetFindingByIdHandler : IRequestHandler<GetFindingByIdQuery, ThreatFinding>
    {
        private readonly IThreatFindingRepository _findingRepository;

        public GetFindingByIdHandler(IThreatFindingRepository findingRepository)
        {
            _findingRepository = findingRepository ?? throw new ArgumentNullException(nameof(findingRepository));
        }

        public async Task<ThreatFinding> Handle(GetFindingByIdQuery request, CancellationToken cancellationToken)
        {
            return await _findingRepository.GetByIdAsync(request.FindingId);
        }
    }

    /// <summary>
    /// Handler for getting all decisions
    /// </summary>
    public class GetAllDecisionsHandler : IRequestHandler<GetAllDecisionsQuery, IEnumerable<AgentDecision>>
    {
        private readonly IAgentDecisionRepository _decisionRepository;

        public GetAllDecisionsHandler(IAgentDecisionRepository decisionRepository)
        {
            _decisionRepository = decisionRepository ?? throw new ArgumentNullException(nameof(decisionRepository));
        }

        public async Task<IEnumerable<AgentDecision>> Handle(GetAllDecisionsQuery request, CancellationToken cancellationToken)
        {
            return await _decisionRepository.GetAllAsync(request.Limit);
        }
    }

    /// <summary>
    /// Handler for getting system health
    /// </summary>
    public class GetSystemHealthHandler : IRequestHandler<GetSystemHealthQuery, Dictionary<string, object>>
    {
        public async Task<Dictionary<string, object>> Handle(GetSystemHealthQuery request, CancellationToken cancellationToken)
        {
            // In a real implementation, this would check actual system health
            var health = new Dictionary<string, object>
            {
                ["status"] = "healthy",
                ["timestamp"] = DateTime.UtcNow,
                ["services"] = new Dictionary<string, string>
                {
                    ["database"] = "up",
                    ["kafka"] = "up",
                    ["api"] = "up"
                }
            };

            if (request.IncludeDetails)
            {
                health["details"] = new Dictionary<string, object>
                {
                    ["uptime"] = "24h 30m",
                    ["memory_usage"] = "45%",
                    ["cpu_usage"] = "23%"
                };
            }

            return health;
        }
    }

    /// <summary>
    /// Handler for getting dashboard statistics
    /// </summary>
    public class GetDashboardStatsHandler : IRequestHandler<GetDashboardStatsQuery, Dictionary<string, object>>
    {
        private readonly INetworkEventRepository _eventRepository;
        private readonly IThreatFindingRepository _findingRepository;
        private readonly IAgentDecisionRepository _decisionRepository;

        public GetDashboardStatsHandler(
            INetworkEventRepository eventRepository,
            IThreatFindingRepository findingRepository,
            IAgentDecisionRepository decisionRepository)
        {
            _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
            _findingRepository = findingRepository ?? throw new ArgumentNullException(nameof(findingRepository));
            _decisionRepository = decisionRepository ?? throw new ArgumentNullException(nameof(decisionRepository));
        }

        public async Task<Dictionary<string, object>> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
        {
            var startTime = request.StartTime ?? DateTime.UtcNow.AddHours(-24);
            var endTime = request.EndTime ?? DateTime.UtcNow;

            var events = await _eventRepository.GetByTimeRangeAsync(startTime, endTime);
            var findings = await _findingRepository.GetByTimeRangeAsync(startTime, endTime);
            var decisions = await _decisionRepository.GetByTimeRangeAsync(startTime, endTime);

            var stats = new Dictionary<string, object>
            {
                ["total_events"] = events.Count(),
                ["total_findings"] = findings.Count(),
                ["total_decisions"] = decisions.Count(),
                ["critical_findings"] = findings.Count(f => f.Severity == "critical"),
                ["high_findings"] = findings.Count(f => f.Severity == "high"),
                ["medium_findings"] = findings.Count(f => f.Severity == "medium"),
                ["low_findings"] = findings.Count(f => f.Severity == "low"),
                ["events_by_source"] = events.GroupBy(e => e.Source)
                    .ToDictionary(g => g.Key, g => g.Count()),
                ["findings_by_severity"] = findings.GroupBy(f => f.Severity)
                    .ToDictionary(g => g.Key, g => g.Count()),
                ["time_range"] = new { start = startTime, end = endTime }
            };

            return stats;
        }
    }
}
