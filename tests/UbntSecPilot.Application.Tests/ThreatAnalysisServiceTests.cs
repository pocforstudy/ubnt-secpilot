using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UbntSecPilot.Application.Services;
using UbntSecPilot.Domain.Models;
using UbntSecPilot.Domain.Repositories;
using Xunit;

namespace UbntSecPilot.Application.Tests
{
    public class ThreatAnalysisServiceTests
    {
        private sealed class InMemoryFindingsRepo : IThreatFindingRepository
        {
            public readonly List<ThreatFinding> Store = new();

            public Task<ThreatFinding> GetByIdAsync(string findingId)
                => Task.FromResult(Store.FirstOrDefault(f => f.Id == findingId)!);

            public Task<IEnumerable<ThreatFinding>> GetAllAsync(int limit = 100)
                => Task.FromResult<IEnumerable<ThreatFinding>>(Store.Take(limit).ToList());

            public Task<IEnumerable<ThreatFinding>> GetBySeverityAsync(string severity, int limit = 100)
                => Task.FromResult<IEnumerable<ThreatFinding>>(Store.Where(f => string.Equals(f.Severity, severity, StringComparison.OrdinalIgnoreCase)).Take(limit).ToList());

            public Task<IEnumerable<ThreatFinding>> GetByTimeRangeAsync(DateTime startTime, DateTime endTime)
                => Task.FromResult<IEnumerable<ThreatFinding>>(Store);

            public Task SaveAsync(ThreatFinding finding)
            {
                Store.Add(finding);
                return Task.CompletedTask;
            }

            public Task SaveManyAsync(IEnumerable<ThreatFinding> findings)
            {
                Store.AddRange(findings);
                return Task.CompletedTask;
            }

            public Task UpdateAsync(ThreatFinding threatFinding)
            {
                var idx = Store.FindIndex(f => f.Id == threatFinding.Id);
                if (idx >= 0) Store[idx] = threatFinding;
                return Task.CompletedTask;
            }

            public Task DeleteAsync(string findingId)
            {
                Store.RemoveAll(f => f.Id == findingId);
                return Task.CompletedTask;
            }
        }

        private sealed class InMemoryDecisionsRepo : IAgentDecisionRepository
        {
            public readonly List<AgentDecision> Store = new();

            public Task<AgentDecision> GetByIdAsync(string decisionId)
                => Task.FromResult(Store.FirstOrDefault()!);

            public Task<IEnumerable<AgentDecision>> GetAllAsync(int limit = 100)
                => Task.FromResult<IEnumerable<AgentDecision>>(Store.Take(limit).ToList());

            public Task<IEnumerable<AgentDecision>> GetByTimeRangeAsync(DateTime startTime, DateTime endTime)
                => Task.FromResult<IEnumerable<AgentDecision>>(Store);

            public Task SaveAsync(AgentDecision decision)
            {
                Store.Add(decision);
                return Task.CompletedTask;
            }

            public Task SaveManyAsync(IEnumerable<AgentDecision> agentDecisions)
            {
                Store.AddRange(agentDecisions);
                return Task.CompletedTask;
            }

            public Task UpdateAsync(AgentDecision agentDecision)
            {
                var idx = Store.FindIndex(d => d.Id == agentDecision.Id);
                if (idx >= 0) Store[idx] = agentDecision;
                return Task.CompletedTask;
            }

            public Task DeleteAsync(string decisionId)
            {
                Store.RemoveAll(d => d.Id == decisionId);
                return Task.CompletedTask;
            }
        }

        private sealed class NoopEventsRepo : INetworkEventRepository
        {
            public Task<NetworkEvent> GetByIdAsync(string eventId) => Task.FromResult<NetworkEvent>(null!);
            public Task<IEnumerable<NetworkEvent>> GetAllAsync(int limit = 100) => Task.FromResult<IEnumerable<NetworkEvent>>(Array.Empty<NetworkEvent>());
            public Task<IEnumerable<NetworkEvent>> GetBySourceAsync(string source, int limit = 100) => Task.FromResult<IEnumerable<NetworkEvent>>(Array.Empty<NetworkEvent>());
            public Task<IEnumerable<NetworkEvent>> GetByTimeRangeAsync(DateTime startTime, DateTime endTime) => Task.FromResult<IEnumerable<NetworkEvent>>(Array.Empty<NetworkEvent>());
            public Task<IEnumerable<NetworkEvent>> GetUnprocessedEventsAsync(int limit = 10) => Task.FromResult<IEnumerable<NetworkEvent>>(Array.Empty<NetworkEvent>());
            public Task SaveAsync(NetworkEvent networkEvent) => Task.CompletedTask;
            public Task SaveManyAsync(IEnumerable<NetworkEvent> networkEvents) => Task.CompletedTask;
            public Task UpdateAsync(NetworkEvent networkEvent) => Task.CompletedTask;
            public Task DeleteAsync(string eventId) => Task.CompletedTask;
        }

        [Fact]
        public async Task AnalyzeNetworkEventAsync_Produces_Finding_When_Suspicious()
        {
            var events = new NoopEventsRepo();
            var findings = new InMemoryFindingsRepo();
            var decisions = new InMemoryDecisionsRepo();
            var svc = new ThreatAnalysisService(events, findings, decisions);

            var ev = new NetworkEvent("evt-x", "unit-test", new Dictionary<string, object>
            {
                ["source_ip"] = "172.16.0.5",
                ["destination_port"] = 4444,
                ["user_agent"] = "crawler"
            }, DateTime.UtcNow);

            var finding = await svc.AnalyzeNetworkEventAsync(ev);

            Assert.NotNull(finding);
            Assert.True(findings.Store.Any());
            Assert.Contains("suspicious", finding!.Summary, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task AnalyzeNetworkEventAsync_Returns_Null_When_Clean()
        {
            var events = new NoopEventsRepo();
            var findings = new InMemoryFindingsRepo();
            var decisions = new InMemoryDecisionsRepo();
            var svc = new ThreatAnalysisService(events, findings, decisions);

            var ev = new NetworkEvent("evt-y", "unit-test", new Dictionary<string, object>(), DateTime.UtcNow);
            var finding = await svc.AnalyzeNetworkEventAsync(ev);

            Assert.Null(finding);
            Assert.Empty(findings.Store);
        }
    }
}
