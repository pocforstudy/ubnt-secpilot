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

            var findings = new List<ThreatFinding>();
            var processedCount = 0;

            foreach (var ev in toProcess)
            {
                // Simple threat detection logic (similar to Orleans version but simplified)
                var isSuspicious = IsSuspiciousEvent(ev);
                NetworkEvent updatedEvent = ev;

                if (isSuspicious)
                {
                    var finding = CreateFindingForEvent(ev);
                    if (finding != null)
                    {
                        findings.Add(finding);
                        await _findings.SaveAsync(finding).ConfigureAwait(false);
                    }
                }

                updatedEvent = updatedEvent.MarkAsProcessed();
                await _events.SaveAsync(updatedEvent).ConfigureAwait(false);
                processedCount++;
            }

            // Create a decision summary
            var decision = new AgentDecision("threat-enrichment", $"processed {processedCount} events, found {findings.Count} threats");
            await _decisions.SaveAsync(decision).ConfigureAwait(false);

            var meta = new Dictionary<string, object>
            {
                ["events_collected"] = toProcess.Count,
                ["events_processed"] = processedCount,
                ["findings_produced"] = findings.Count
            };
            return ($"processed {processedCount} events, found {findings.Count} threats", meta);
        }

        private bool IsSuspiciousEvent(NetworkEvent networkEvent)
        {
            var payload = networkEvent.Payload;
            var suspiciousIndicators = new List<string>();

            // Check for suspicious ports
            if (payload.TryGetValue("destination_port", out var destPortObj) &&
                destPortObj != null)
            {
                if (int.TryParse(destPortObj.ToString(), out var destPort))
                {
                    var badPorts = new[] { 4444, 6667, 31337, 12345 };
                    if (badPorts.Contains(destPort))
                    {
                        suspiciousIndicators.Add("suspicious_destination_port");
                    }
                }
            }

            // Check for suspicious user agents
            if (payload.TryGetValue("user_agent", out var uaObj))
            {
                var ua = uaObj?.ToString()?.ToLowerInvariant() ?? string.Empty;
                var patterns = new[] { "bot", "crawler", "scanner", "exploit" };
                if (patterns.Any(p => ua.Contains(p)))
                {
                    suspiciousIndicators.Add("suspicious_user_agent");
                }
            }

            // Check for private source IPs (but not common test IPs)
            if (payload.TryGetValue("source_ip", out var ipObj))
            {
                var ip = ipObj?.ToString() ?? string.Empty;
                if (ip.StartsWith("192.168.") || ip.StartsWith("10.") || ip.StartsWith("172."))
                {
                    // Only flag as suspicious if it's not a common test IP
                    if (ip != "10.0.0.1" && ip != "192.168.1.1")
                    {
                        suspiciousIndicators.Add("private_source_ip");
                    }
                }
            }

            return suspiciousIndicators.Count > 0;
        }

        private ThreatFinding? CreateFindingForEvent(NetworkEvent networkEvent)
        {
            var payload = networkEvent.Payload;
            var suspiciousIndicators = new List<string>();

            if (payload.TryGetValue("destination_port", out var destPortObj) &&
                destPortObj != null)
            {
                if (int.TryParse(destPortObj.ToString(), out var destPort))
                {
                    var badPorts = new[] { 4444, 6667, 31337, 12345 };
                    if (badPorts.Contains(destPort))
                    {
                        suspiciousIndicators.Add("suspicious_destination_port");
                    }
                }
            }

            if (payload.TryGetValue("user_agent", out var uaObj))
            {
                var ua = uaObj?.ToString()?.ToLowerInvariant() ?? string.Empty;
                var patterns = new[] { "bot", "crawler", "scanner", "exploit" };
                if (patterns.Any(p => ua.Contains(p)))
                {
                    suspiciousIndicators.Add("suspicious_user_agent");
                }
            }

            if (payload.TryGetValue("source_ip", out var ipObj))
            {
                var ip = ipObj?.ToString() ?? string.Empty;
                if (ip.StartsWith("192.168.") || ip.StartsWith("10.") || ip.StartsWith("172."))
                {
                    if (ip != "10.0.0.1" && ip != "192.168.1.1")
                    {
                        suspiciousIndicators.Add("private_source_ip");
                    }
                }
            }

            if (suspiciousIndicators.Count == 0)
            {
                return null;
            }

            var severity = DetermineSeverity(suspiciousIndicators);
            var reason = $"Detected {string.Join(", ", suspiciousIndicators)} in network event";

            return new ThreatFinding(
                networkEvent.EventId,
                severity,
                reason,
                new Dictionary<string, object>
                {
                    ["indicators"] = suspiciousIndicators,
                    ["source_event"] = networkEvent.EventId,
                    ["analyzed_at"] = DateTime.UtcNow
                }
            );
        }

        private string DetermineSeverity(IReadOnlyCollection<string> indicators)
        {
            if (indicators.Count >= 3) return "critical";
            if (indicators.Count >= 2) return "high";
            if (indicators.Any(i => i.Contains("ip") || i.Contains("port"))) return "medium";
            return "low";
        }
    }
}
