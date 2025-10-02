using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UbntSecPilot.Application.Services;
using UbntSecPilot.Domain.Models;
using Xunit;

namespace UbntSecPilot.Application.Tests
{
    public class PreAnalysisServiceTests
    {
        [Fact]
        public async Task Flags_Suspicious_Port_And_UserAgent()
        {
            var svc = new PreAnalysisService();
            var ev = new NetworkEvent(
                eventId: "e1",
                source: "test",
                payload: new Dictionary<string, object>
                {
                    ["destination_port"] = 31337,
                    ["user_agent"] = "some-malicious-bot"
                },
                occurredAt: DateTime.UtcNow
            );

            var (isSuspicious, reason, signals) = await svc.AnalyzeAsync(ev);

            Assert.True(isSuspicious);
            Assert.Contains("bad_port:31337", reason);
            Assert.Contains("suspicious_user_agent", reason);
            Assert.True(signals.ContainsKey("bad_port_match"));
            Assert.True(signals.ContainsKey("user_agent_pattern"));
        }

        [Fact]
        public async Task Clean_Event_When_No_Signals()
        {
            var svc = new PreAnalysisService();
            var ev = new NetworkEvent("e2", "test", new Dictionary<string, object>(), DateTime.UtcNow);

            var (isSuspicious, reason, _) = await svc.AnalyzeAsync(ev);

            Assert.False(isSuspicious);
            Assert.Equal("clean", reason);
        }
    }
}
