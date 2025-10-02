using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UbntSecPilot.Agents;
using UbntSecPilot.Domain.Models;
using UbntSecPilot.Domain.Repositories;
using UbntSecPilot.Domain.ValueObjects;

namespace UbntSecPilot.Agents
{
    public sealed class ThreatEnrichmentAgent : BaseAgent
    {
        private readonly INetworkEventRepository _events;
        private readonly IThreatFindingRepository _findings;
        private readonly IAgentDecisionRepository _decisions;
        private readonly ILogger<ThreatEnrichmentAgent> _logger;

        public ThreatEnrichmentAgent(
            INetworkEventRepository events,
            IThreatFindingRepository findings,
            IAgentDecisionRepository decisions,
            ILogger<ThreatEnrichmentAgent> logger)
            : base("threat-enrichment")
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _findings = findings ?? throw new ArgumentNullException(nameof(findings));
            _decisions = decisions ?? throw new ArgumentNullException(nameof(decisions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task<(string reason, IDictionary<string, object> metadata)> LoopAsync(CancellationToken cancellationToken)
        {
            var toProcess = (await _events.GetUnprocessedEventsAsync(50).ConfigureAwait(false))?.ToList() ?? new List<NetworkEvent>();
            if (toProcess.Count == 0)
            {
                _logger.LogInformation("No events to process");
                return ("no events", new Dictionary<string, object> {
                    ["events_collected"] = 0,
                    ["findings_produced"] = 0
                });
            }

            _logger.LogInformation("Processing {Count} events", toProcess.Count);

            // Simple placeholder enrichment: mark as processed; in future integrate Application services
            foreach (var ev in toProcess)
            {
                ev.MarkAsProcessed();
                await _events.SaveAsync(ev).ConfigureAwait(false);
            }

            // Optionally create a decision summary
            var decision = new AgentDecision("threat-enrichment", $"processed {toProcess.Count} events");
            await _decisions.SaveAsync(decision).ConfigureAwait(false);

            var meta = new Dictionary<string, object>
            {
                ["events_collected"] = toProcess.Count,
                ["findings_produced"] = 0
            };
            return ($"processed {toProcess.Count} events", meta);
        }
    }
}
