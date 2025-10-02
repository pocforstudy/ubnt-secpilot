using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UbntSecPilot.Domain.Models;

namespace UbntSecPilot.Application.Services
{
    /// <summary>
    /// Cheap heuristics pre-analysis executed before heavy enrichment.
    /// Returns a simple decision about whether an event is suspicious.
    /// </summary>
    public class PreAnalysisService
    {
        public Task<(bool isSuspicious, string reason, IReadOnlyDictionary<string, object> signals)> AnalyzeAsync(NetworkEvent ev)
        {
            if (ev == null) throw new ArgumentNullException(nameof(ev));
            var payload = ev.Payload ?? new Dictionary<string, object>();
            var signals = new Dictionary<string, object>();
            var reasons = new List<string>();

            if (payload.TryGetValue("destination_port", out var destPortObj) &&
                int.TryParse(destPortObj?.ToString(), out var destPort))
            {
                var badPorts = new[] { 4444, 6667, 31337, 12345 };
                if (badPorts.Contains(destPort))
                {
                    reasons.Add($"bad_port:{destPort}");
                    signals["bad_port_match"] = destPort;
                }
            }

            if (payload.TryGetValue("user_agent", out var uaObj))
            {
                var ua = uaObj?.ToString()?.ToLowerInvariant() ?? string.Empty;
                var patterns = new[] { "bot", "crawler", "scanner", "exploit" };
                if (patterns.Any(p => ua.Contains(p)))
                {
                    reasons.Add("suspicious_user_agent");
                    signals["user_agent_pattern"] = ua;
                }
            }

            if (payload.TryGetValue("source_ip", out var ipObj))
            {
                var ip = ipObj?.ToString() ?? string.Empty;
                if (ip.StartsWith("192.168.") || ip.StartsWith("10.") || ip.StartsWith("172."))
                {
                    reasons.Add("private_source_ip");
                    signals["private_ip"] = true;
                }
            }

            var isSuspicious = reasons.Count > 0;
            var reason = isSuspicious ? string.Join(",", reasons) : "clean";
            return Task.FromResult<(bool, string, IReadOnlyDictionary<string, object>)>((isSuspicious, reason, signals));
        }
    }
}
