using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UbntSecPilot.Domain.Models;
using UbntSecPilot.Domain.Repositories;
using UbntSecPilot.Domain.ValueObjects;

namespace UbntSecPilot.Application.Services
{
    /// <summary>
    /// Application service for threat analysis
    /// </summary>
    public class ThreatAnalysisService : IThreatAnalysisService
    {
        private readonly INetworkEventRepository _eventRepository;
        private readonly IThreatFindingRepository _findingRepository;
        private readonly IAgentDecisionRepository _decisionRepository;

        public ThreatAnalysisService(
            INetworkEventRepository eventRepository,
            IThreatFindingRepository findingRepository,
            IAgentDecisionRepository decisionRepository)
        {
            _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
            _findingRepository = findingRepository ?? throw new ArgumentNullException(nameof(findingRepository));
            _decisionRepository = decisionRepository ?? throw new ArgumentNullException(nameof(decisionRepository));
        }

        public async Task<ThreatFinding?> AnalyzeNetworkEventAsync(NetworkEvent networkEvent)
        {
            // Simple threat analysis logic
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
            
            var finding = new ThreatFinding(
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
            
            await _findingRepository.SaveAsync(finding);
            
            var decision = new AgentDecision("threat-analysis", $"analyzed event {networkEvent.EventId}, found {suspiciousIndicators.Count} indicators");
            await _decisionRepository.SaveAsync(decision);
            
            return finding;
        }
        public async Task<AgentDecision> RunThreatEnrichmentAsync()
        {
            var decision = new AgentDecision("threat-enrichment", "completed threat enrichment process");
            await _decisionRepository.SaveAsync(decision);
            return decision;
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

    /// <summary>
    /// Application service for thread analysis
    /// </summary>
    public class ThreadAnalysisService : IThreadAnalysisService
    {
        private readonly IThreadAnalysisRepository _threadAnalysisRepository;

        public ThreadAnalysisService(IThreadAnalysisRepository threadAnalysisRepository)
        {
            _threadAnalysisRepository = threadAnalysisRepository ?? throw new ArgumentNullException(nameof(threadAnalysisRepository));
        }

        public async Task<ThreadAnalysis> AnalyzeThreadAsync(string threadId, bool isIoc, string severity, string reason, List<string> indicators)
        {
            var analysis = new ThreadAnalysis(threadId, isIoc, severity, reason, indicators);
            await _threadAnalysisRepository.SaveAsync(analysis);
            return analysis;
        }
    }
}
