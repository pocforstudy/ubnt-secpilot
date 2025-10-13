using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Orleans;
using UbntSecPilot.Application.Services;
using UbntSecPilot.Agents.Orleans.Transactions;
using UbntSecPilot.Domain.Models;
using UbntSecPilot.Domain.Repositories;
using UbntSecPilot.Domain.ValueObjects;
namespace UbntSecPilot.Agents.Orleans
{
    public class ThreatEnrichmentAgentGrain : AgentGrain, IAgentGrain
    {
        private readonly INetworkEventRepository _events;
        private readonly IThreatFindingRepository _findings;
        private readonly IAgentDecisionRepository _decisions;
        private readonly PreAnalysisService _preAnalysis;
        private readonly bool _use2pc;
 
        public ThreatEnrichmentAgentGrain(
            INetworkEventRepository events,
            IThreatFindingRepository findings,
            IAgentDecisionRepository decisions,
            PreAnalysisService preAnalysis,
            ILogger<ThreatEnrichmentAgentGrain> logger,
            IConfiguration config)
            : base(logger)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _findings = findings ?? throw new ArgumentNullException(nameof(findings));
            _decisions = decisions ?? throw new ArgumentNullException(nameof(decisions));
            _preAnalysis = preAnalysis ?? throw new ArgumentNullException(nameof(preAnalysis));
            _use2pc = config?.GetValue<bool>("Agents:Use2PC") ?? true;
        }

        protected override async Task<AgentResult> ExecuteAsync(CancellationToken cancellationToken)
        {
            await Task.Yield(); // Ensure proper async execution
            var toProcess = (await _events.GetUnprocessedEventsAsync(50).ConfigureAwait(false))?.ToList() ?? new List<NetworkEvent>();
            if (toProcess.Count == 0)
            {
                Logger.LogInformation("No events to process");
                return new AgentResult("threat-enrichment", "no events", new System.Collections.ObjectModel.ReadOnlyDictionary<string, object>(new Dictionary<string, object>
                {
                    ["events_collected"] = 0,
                    ["findings_produced"] = 0
                }));
            }

            Logger.LogInformation("Processing {Count} events", toProcess.Count);

            var findings = new List<ThreatFinding>();
            var processedCount = 0;

            foreach (var ev in toProcess)
            {
                try
                {
                    var (isSuspicious, preReason, signals) = await _preAnalysis.AnalyzeAsync(ev).ConfigureAwait(false);
                    if (!isSuspicious)
                    {
                        ev.MarkAsProcessed();
                        await _events.SaveAsync(ev).ConfigureAwait(false);
                        processedCount++;
                        continue;
                    }
                    var finding = await AnalyzeEventAsync(ev).ConfigureAwait(false);
                    if (finding != null)
                    {
                        
                        if (signals != null && signals.Count > 0)
                        {
                            finding.Metadata["pre_signals"] = signals;
                            finding.Metadata["pre_reason"] = preReason;
                        }
                        findings.Add(finding);
                    }

                    ev.MarkAsProcessed();
                    if (_use2pc)
                    {
                        // 2PC per event (event + optional finding)
                        var txPayload = new Transactions.TransactionPayload
                        {
                            TransactionId = Guid.NewGuid().ToString("N"),
                            UpdatedEvent = ev,
                            Finding = finding,
                            Metadata = new Dictionary<string, object>{{"threat-enrichment","threat-enrichment"}}
                        };
                        var participants = new List<string> { $"event:{ev.EventId}" };
                        if (finding != null)
                        {
                            participants.Add($"finding:{ev.EventId}");
                        }
                        var coordinator = GrainFactory.GetGrain<Transactions.ITransactionCoordinatorGrain>("tx-coordinator");
                        var ok = await coordinator.RunTwoPhaseCommitAsync(txPayload, participants).ConfigureAwait(false);
                        if (!ok)
                        {
                            Logger.LogWarning("2PC failed for event {EventId}", ev.EventId);
                        }
                    }
                    else
                    {
                        // Direct write fallback when 2PC disabled
                        await _events.SaveAsync(ev).ConfigureAwait(false);
                    }
                    processedCount++;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error processing event {EventId}", ev.EventId);
                }
            }

            var decision = new AgentDecision("threat-enrichment", $"processed {processedCount} events, found {findings.Count} threats");
            if (_use2pc)
            {
                // Record decision via 2PC (decision participant only)
                var decisionPayload = new Transactions.TransactionPayload
                {
                    TransactionId = Guid.NewGuid().ToString("N"),
                    Decision = decision
                };
                var decisionParticipants = new List<string> { $"decision:run-{DateTime.UtcNow:yyyyMMddHHmmss}" };
                var decisionCoordinator = GrainFactory.GetGrain<Transactions.ITransactionCoordinatorGrain>("tx-coordinator");
                await decisionCoordinator.RunTwoPhaseCommitAsync(decisionPayload, decisionParticipants).ConfigureAwait(false);
            }
            else
            {
                if (findings.Any())
                {
                    await _findings.SaveManyAsync(findings).ConfigureAwait(false);
                }
                await _decisions.SaveAsync(decision).ConfigureAwait(false);
            }

            var metadata = new Dictionary<string, object>
            {
                ["events_collected"] = toProcess.Count,
                ["events_processed"] = processedCount,
                ["findings_produced"] = findings.Count,
                ["threat_severity_breakdown"] = findings.GroupBy(f => f.Severity).ToDictionary(g => g.Key, g => g.Count())
            };

            return new AgentResult("threat-enrichment", $"processed {processedCount} events, found {findings.Count} threats", new System.Collections.ObjectModel.ReadOnlyDictionary<string, object>(metadata));
        }

        private async Task<ThreatFinding?> AnalyzeEventAsync(NetworkEvent networkEvent)
        {
            await Task.Yield(); // Ensure proper async execution

            var payload = networkEvent.Payload;
            var suspiciousIndicators = new List<string>();

            if (payload.TryGetValue("source_ip", out var sourceIp) && IsSuspiciousIp(sourceIp?.ToString()))
            {
                suspiciousIndicators.Add("suspicious_source_ip");
            }

            if (payload.TryGetValue("destination_port", out var destPort) && IsSuspiciousPort(destPort))
            {
                suspiciousIndicators.Add("suspicious_destination_port");
            }

            if (payload.TryGetValue("user_agent", out var userAgent) && IsSuspiciousUserAgent(userAgent?.ToString()))
            {
                suspiciousIndicators.Add("suspicious_user_agent");
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

        private bool IsSuspiciousIp(string? ip)
        {
            if (string.IsNullOrEmpty(ip)) return false;
            return ip.StartsWith("192.168.") || ip.StartsWith("10.") || ip.StartsWith("172.");
        }

        private bool IsSuspiciousPort(object? portObj)
        {
            if (portObj == null) return false;
            if (int.TryParse(portObj.ToString(), out var port))
            {
                var suspiciousPorts = new[] { 4444, 6667, 31337, 12345 };
                return suspiciousPorts.Contains(port);
            }
            return false;
        }

        private bool IsSuspiciousUserAgent(string? userAgent)
        {
            if (string.IsNullOrEmpty(userAgent)) return false;
            var suspiciousPatterns = new[] { "bot", "crawler", "scanner", "exploit" };
            return suspiciousPatterns.Any(pattern => userAgent.ToLower().Contains(pattern));
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
