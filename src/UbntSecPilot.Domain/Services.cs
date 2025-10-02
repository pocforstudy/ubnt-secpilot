using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UbntSecPilot.Domain.Models;
using UbntSecPilot.Domain.Repositories;

namespace UbntSecPilot.Domain.Services
{
    /// <summary>
    /// Domain service for threat analysis business logic
    /// </summary>
    public class ThreatAnalysisService
    {
        private readonly INetworkEventRepository _eventRepository;
        private readonly IThreatFindingRepository _findingRepository;

        public ThreatAnalysisService(INetworkEventRepository eventRepository, IThreatFindingRepository findingRepository)
        {
            _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
            _findingRepository = findingRepository ?? throw new ArgumentNullException(nameof(findingRepository));
        }

        public async Task<ThreatFinding> AnalyzeNetworkEventAsync(NetworkEvent networkEvent)
        {
            if (networkEvent == null)
                throw new ArgumentNullException(nameof(networkEvent));

            // Business logic for threat analysis
            var severity = DetermineSeverity(networkEvent);
            var summary = GenerateSummary(networkEvent);

            if (severity == "info")
            {
                return null; // No threat detected
            }

            var metadata = new Dictionary<string, object>
            {
                ["analysis_timestamp"] = DateTime.UtcNow,
                ["confidence_score"] = CalculateConfidenceScore(networkEvent),
                ["indicators"] = ExtractIndicators(networkEvent)
            };

            var finding = new ThreatFinding(networkEvent.EventId, severity, summary, metadata);

            await _findingRepository.SaveAsync(finding);

            return finding;
        }

        public async Task<AgentDecision> RunThreatEnrichmentAsync()
        {
            var unprocessedEvents = await _eventRepository.GetUnprocessedEventsAsync(10);

            var processedCount = 0;
            var findingsCount = 0;

            foreach (var networkEvent in unprocessedEvents)
            {
                try
                {
                    var finding = await AnalyzeNetworkEventAsync(networkEvent);

                    if (finding != null)
                    {
                        findingsCount++;
                    }

                    networkEvent.MarkAsProcessed();
                    await _eventRepository.UpdateAsync(networkEvent);

                    processedCount++;
                }
                catch (Exception ex)
                {
                    networkEvent.MarkAsFailed(ex.Message);
                    await _eventRepository.UpdateAsync(networkEvent);
                }
            }

            return new AgentDecision(
                "threat_enrichment_completed",
                $"Processed {processedCount} events, generated {findingsCount} findings",
                new Dictionary<string, object>
                {
                    ["events_processed"] = processedCount,
                    ["findings_generated"] = findingsCount,
                    ["timestamp"] = DateTime.UtcNow
                }
            );
        }

        private string DetermineSeverity(NetworkEvent networkEvent)
        {
            // Business logic for severity determination
            var payload = networkEvent.Payload;

            if (payload.ContainsKey("severity"))
            {
                return payload["severity"].ToString();
            }

            if (payload.ContainsKey("threat_level"))
            {
                var threatLevel = payload["threat_level"].ToString().ToLower();
                return threatLevel switch
                {
                    "critical" => "critical",
                    "high" => "high",
                    "medium" => "medium",
                    _ => "low"
                };
            }

            return "info";
        }

        private string GenerateSummary(NetworkEvent networkEvent)
        {
            var payload = networkEvent.Payload;

            if (payload.ContainsKey("summary"))
            {
                return payload["summary"].ToString();
            }

            var indicator = payload.TryGetValue("indicator", out var indicatorValue)
                ? indicatorValue.ToString() : "unknown_event";

            return $"{networkEvent.Source} detected {indicator} at {networkEvent.OccurredAt:g}";
        }

        private double CalculateConfidenceScore(NetworkEvent networkEvent)
        {
            // Business logic for confidence calculation
            var score = 0.5; // Base score

            var payload = networkEvent.Payload;

            if (payload.ContainsKey("confidence"))
            {
                if (double.TryParse(payload["confidence"].ToString(), out var confidence))
                {
                    score = confidence;
                }
            }

            if (payload.ContainsKey("source_reliability"))
            {
                if (double.TryParse(payload["source_reliability"].ToString(), out var reliability))
                {
                    score *= reliability;
                }
            }

            return Math.Max(0.0, Math.Min(1.0, score));
        }

        private List<string> ExtractIndicators(NetworkEvent networkEvent)
        {
            var indicators = new List<string>();
            var payload = networkEvent.Payload;

            if (payload.ContainsKey("indicators") && payload["indicators"] is List<object> indicatorList)
            {
                indicators.AddRange(indicatorList.Select(i => i.ToString()));
            }

            if (payload.ContainsKey("indicator"))
            {
                indicators.Add(payload["indicator"].ToString());
            }

            return indicators.Distinct().ToList();
        }
    }

    /// <summary>
    /// Domain service for thread analysis business logic
    /// </summary>
    public class ThreadAnalysisService
    {
        private readonly IThreadAnalysisRepository _threadAnalysisRepository;

        public ThreadAnalysisService(IThreadAnalysisRepository threadAnalysisRepository)
        {
            _threadAnalysisRepository = threadAnalysisRepository ?? throw new ArgumentNullException(nameof(threadAnalysisRepository));
        }

        public async Task<ThreadAnalysis> AnalyzeThreadAsync(List<ThreadMessage> messages)
        {
            if (messages == null || !messages.Any())
                throw new ArgumentNullException(nameof(messages));

            var threadId = Guid.NewGuid().ToString();
            var isIoc = DetectIndicatorsOfCompromise(messages);
            var severity = DetermineSeverity(messages);
            var reason = GenerateAnalysisReason(messages, isIoc);
            var indicators = ExtractIndicators(messages);

            var analysis = new ThreadAnalysis(threadId, isIoc, severity, reason, indicators, null);

            await _threadAnalysisRepository.SaveAsync(analysis);

            return analysis;
        }

        private bool DetectIndicatorsOfCompromise(List<ThreadMessage> messages)
        {
            var content = string.Join(" ", messages.Select(m => m.Content)).ToLower();

            var iocPatterns = new[]
            {
                "c2 server", "command and control", "beacon", "malware", "exploit",
                "suspicious ip", "unknown connection", "data exfiltration"
            };

            return iocPatterns.Any(pattern => content.Contains(pattern));
        }

        private string DetermineSeverity(List<ThreadMessage> messages)
        {
            var content = string.Join(" ", messages.Select(m => m.Content)).ToLower();

            if (content.Contains("critical") || content.Contains("urgent"))
                return "critical";

            if (content.Contains("high") || content.Contains("suspicious"))
                return "high";

            if (content.Contains("medium") || content.Contains("warning"))
                return "medium";

            return "low";
        }

        private string GenerateAnalysisReason(List<ThreadMessage> messages, bool isIoc)
        {
            if (isIoc)
            {
                return "Potential indicators of compromise detected in thread content";
            }

            return "No indicators of compromise detected in thread content";
        }

        private List<string> ExtractIndicators(List<ThreadMessage> messages)
        {
            var indicators = new List<string>();
            var content = string.Join(" ", messages.Select(m => m.Content));

            // Simple indicator extraction - in real implementation, use more sophisticated NLP
            var potentialIndicators = new[]
            {
                "ip address", "domain", "url", "hash", "file", "registry key"
            };

            foreach (var pattern in potentialIndicators)
            {
                if (content.Contains(pattern))
                {
                    indicators.Add(pattern);
                }
            }

            return indicators;
        }
    }

    /// <summary>
    /// Repository interfaces for DDD
    /// </summary>
    public interface IThreatAnalysisService
    {
        Task<ThreatFinding> AnalyzeNetworkEventAsync(NetworkEvent networkEvent);
        Task<AgentDecision> RunThreatEnrichmentAsync();
    }

    public interface IThreadAnalysisService
    {
        Task<ThreadAnalysis> AnalyzeThreadAsync(List<ThreadMessage> messages);
    }
}
