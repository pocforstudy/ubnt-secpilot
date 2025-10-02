using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Orleans.Hosting;
using Orleans.TestingHost;
using UbntSecPilot.Agents.Orleans.Transactions;
using UbntSecPilot.Domain.Models;
using UbntSecPilot.Domain.Repositories;
using UbntSecPilot.Infrastructure.Transactions;
using Xunit;

namespace UbntSecPilot.Orleans.Tests
{
    public class InMemoryOutboxRepository : IOutboxRepository
    {
        private readonly ConcurrentDictionary<(string tx, string key), OutboxRecord> _store = new();
        public Task SavePreparedAsync(OutboxRecord record)
        {
            record.Status = "prepared";
            _store[(record.TxId, record.ParticipantKey)] = record;
            return Task.CompletedTask;
        }
        public Task MarkCommittedAsync(string txId, string participantKey)
        {
            _store.AddOrUpdate((txId, participantKey),
                _ => new OutboxRecord { TxId = txId, ParticipantKey = participantKey, Status = "committed" },
                (_, rec) => { rec.Status = "committed"; return rec; });
            return Task.CompletedTask;
        }
        public Task MarkAbortedAsync(string txId, string participantKey)
        {
            _store.AddOrUpdate((txId, participantKey),
                _ => new OutboxRecord { TxId = txId, ParticipantKey = participantKey, Status = "aborted" },
                (_, rec) => { rec.Status = "aborted"; return rec; });
            return Task.CompletedTask;
        }
        public Task<OutboxRecord?> GetAsync(string txId, string participantKey)
        {
            _store.TryGetValue((txId, participantKey), out var rec);
            return Task.FromResult<OutboxRecord?>(rec);
        }
        public IReadOnlyDictionary<(string tx, string key), OutboxRecord> Snapshot() => _store;
    }

    public class InMemoryEventsRepo : INetworkEventRepository
    {
        public readonly ConcurrentDictionary<string, NetworkEvent> Store = new();
        public Task<NetworkEvent> GetByIdAsync(string eventId) => Task.FromResult(Store.TryGetValue(eventId, out var ev) ? ev : null!);
        public Task<IEnumerable<NetworkEvent>> GetAllAsync(int limit = 100) => Task.FromResult<IEnumerable<NetworkEvent>>(Store.Values);
        public Task<IEnumerable<NetworkEvent>> GetUnprocessedEventsAsync(int limit = 10) => Task.FromResult<IEnumerable<NetworkEvent>>(Array.Empty<NetworkEvent>());
        public Task SaveAsync(NetworkEvent networkEvent) { Store[networkEvent.EventId] = networkEvent; return Task.CompletedTask; }
        public Task SaveManyAsync(IEnumerable<NetworkEvent> networkEvents) { foreach (var ev in networkEvents) Store[ev.EventId] = ev; return Task.CompletedTask; }
        public Task UpdateAsync(NetworkEvent networkEvent) { Store[networkEvent.EventId] = networkEvent; return Task.CompletedTask; }
        public Task DeleteAsync(string eventId) { Store.TryRemove(eventId, out _); return Task.CompletedTask; }
        public Task<IEnumerable<NetworkEvent>> GetBySourceAsync(string source, int limit = 100) => Task.FromResult<IEnumerable<NetworkEvent>>(Array.Empty<NetworkEvent>());
        public Task<IEnumerable<NetworkEvent>> GetByTimeRangeAsync(DateTime start, DateTime end) => Task.FromResult<IEnumerable<NetworkEvent>>(Array.Empty<NetworkEvent>());
    }

    public class InMemoryFindingsRepo : IThreatFindingRepository
    {
        public readonly List<ThreatFinding> Store = new();
        public Task<ThreatFinding> GetByIdAsync(string findingId) => Task.FromResult(Store.FirstOrDefault(f => f.Id == findingId)!);
        public Task<IEnumerable<ThreatFinding>> GetAllAsync(int limit = 100) => Task.FromResult<IEnumerable<ThreatFinding>>(Store.Take(limit).ToList());
        public Task<IEnumerable<ThreatFinding>> GetBySeverityAsync(string severity, int limit = 100) => Task.FromResult<IEnumerable<ThreatFinding>>(Store.Where(f => string.Equals(f.Severity, severity, StringComparison.OrdinalIgnoreCase)).Take(limit).ToList());
        public Task<IEnumerable<ThreatFinding>> GetByTimeRangeAsync(DateTime startTime, DateTime endTime) => Task.FromResult<IEnumerable<ThreatFinding>>(Store);
        public Task SaveAsync(ThreatFinding finding) { Store.Add(finding); return Task.CompletedTask; }
        public Task SaveManyAsync(IEnumerable<ThreatFinding> findings) { Store.AddRange(findings); return Task.CompletedTask; }
        public Task UpdateAsync(ThreatFinding threatFinding) { var idx = Store.FindIndex(f => f.Id == threatFinding.Id); if (idx >= 0) Store[idx] = threatFinding; return Task.CompletedTask; }
        public Task DeleteAsync(string findingId) { Store.RemoveAll(f => f.Id == findingId); return Task.CompletedTask; }
    }

    public class InMemoryDecisionsRepo : IAgentDecisionRepository
    {
        public readonly List<AgentDecision> Store = new();
        public Task<AgentDecision> GetByIdAsync(string decisionId) => Task.FromResult(Store.FirstOrDefault(d => d.Id == decisionId)!);
        public Task<IEnumerable<AgentDecision>> GetAllAsync(int limit = 100) => Task.FromResult<IEnumerable<AgentDecision>>(Store.Take(limit).ToList());
        public Task<IEnumerable<AgentDecision>> GetByTimeRangeAsync(DateTime start, DateTime end) => Task.FromResult<IEnumerable<AgentDecision>>(Store);
        public Task SaveAsync(AgentDecision decision) { Store.Add(decision); return Task.CompletedTask; }
        public Task SaveManyAsync(IEnumerable<AgentDecision> decisions) { Store.AddRange(decisions); return Task.CompletedTask; }
        public Task UpdateAsync(AgentDecision agentDecision) { var idx = Store.FindIndex(d => d.Id == agentDecision.Id); if (idx >= 0) Store[idx] = agentDecision; return Task.CompletedTask; }
        public Task DeleteAsync(string decisionId) { Store.RemoveAll(d => d.Id == decisionId); return Task.CompletedTask; }
    }

    public class SiloConfigurator : ISiloConfigurator
    {
        public void Configure(ISiloBuilder siloBuilder)
        {
            siloBuilder
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IOutboxRepository, InMemoryOutboxRepository>();
                    services.AddSingleton<INetworkEventRepository, InMemoryEventsRepo>();
                    services.AddSingleton<IThreatFindingRepository, InMemoryFindingsRepo>();
                    services.AddSingleton<IAgentDecisionRepository, InMemoryDecisionsRepo>();
                })
                ;
        }
    }

    public class CoordinatorParticipantTests : IDisposable
    {
        private readonly TestCluster _cluster;
        public CoordinatorParticipantTests()
        {
            var builder = new TestClusterBuilder();
            builder.AddSiloBuilderConfigurator<SiloConfigurator>();
            _cluster = builder.Build();
            _cluster.Deploy();
        }
        public void Dispose() => _cluster.StopAllSilos();

        [Fact]
        public async Task TwoPhaseCommit_Commits_All_Participants()
        {
            var txId = Guid.NewGuid().ToString("N");
            var ev = new NetworkEvent("evt-2pc", "test", new Dictionary<string, object>{{"k","v"}}, DateTime.UtcNow);
            var finding = new ThreatFinding(ev.EventId, "high", "unit-test", new Dictionary<string, object>());
            var payload = new TransactionPayload
            {
                TransactionId = txId,
                UpdatedEvent = ev,
                Finding = finding
            };
            var participants = new List<string> { $"event:{ev.EventId}", $"finding:{ev.EventId}" };

            var coord = _cluster.GrainFactory.GetGrain<ITransactionCoordinatorGrain>("tx-coordinator");
            var ok = await coord.RunTwoPhaseCommitAsync(payload, participants);
            Assert.True(ok);

            // Validate side-effects via DI singletons
            var outbox = (InMemoryOutboxRepository)_cluster.ServiceProvider.GetRequiredService<IOutboxRepository>();
            Assert.Contains(outbox.Snapshot().Values, r => r.TxId == txId && r.ParticipantKey == $"event:{ev.EventId}" && r.Status == "committed");
            Assert.Contains(outbox.Snapshot().Values, r => r.TxId == txId && r.ParticipantKey == $"finding:{ev.EventId}" && r.Status == "committed");

            var eventsRepo = (InMemoryEventsRepo)_cluster.ServiceProvider.GetRequiredService<INetworkEventRepository>();
            Assert.True(eventsRepo.Store.ContainsKey(ev.EventId));

            var findingsRepo = (InMemoryFindingsRepo)_cluster.ServiceProvider.GetRequiredService<IThreatFindingRepository>();
            Assert.True(findingsRepo.Store.Any(f => f.EventId == ev.EventId));
        }
    }
}
