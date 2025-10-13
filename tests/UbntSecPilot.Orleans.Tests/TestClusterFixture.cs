using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.TestingHost;
using UbntSecPilot.Agents;
using UbntSecPilot.Agents.Orleans;
using UbntSecPilot.Agents.Orleans.Transactions;
using UbntSecPilot.Application.Services;
using UbntSecPilot.Domain.Models;
using UbntSecPilot.Domain.Repositories;
using UbntSecPilot.Domain.ValueObjects;
using UbntSecPilot.Infrastructure.Transactions;
using Xunit;

namespace UbntSecPilot.Orleans.Tests;

public sealed class OrleansTestClusterFixture : IDisposable
{
    private static readonly TestOutboxRepository OutboxRepositoryInstance = new();
    private static readonly TestNetworkEventRepository NetworkEventRepositoryInstance = new();
    private static readonly TestThreatFindingRepository ThreatFindingRepositoryInstance = new();
    private static readonly TestAgentDecisionRepository AgentDecisionRepositoryInstance = new();

    public TestCluster Cluster { get; }
    public IServiceProvider Services => Cluster.ServiceProvider;

    public OrleansTestClusterFixture()
    {
        var builder = new TestClusterBuilder();
        builder.AddSiloBuilderConfigurator<TestSiloConfigurator>();
        builder.AddClientBuilderConfigurator<TestClientConfigurator>();
        Cluster = builder.Build();
        Cluster.Deploy();
    }

    public void ResetState()
    {
        OutboxRepositoryInstance.Reset();
        NetworkEventRepositoryInstance.Reset();
        ThreatFindingRepositoryInstance.Reset();
        AgentDecisionRepositoryInstance.Reset();
    }

    public void SetUse2PC(bool enabled)
    {
        var configuration = (IConfigurationRoot)Services.GetRequiredService<IConfiguration>();
        configuration["Agents:Use2PC"] = enabled ? "true" : "false";
    }

    public T GetService<T>() where T : notnull
    {
        var requestedType = typeof(T);

        if (requestedType == typeof(TestOutboxRepository) || requestedType == typeof(IOutboxRepository))
        {
            return (T)(object)OutboxRepositoryInstance;
        }

        if (requestedType == typeof(TestNetworkEventRepository) || requestedType == typeof(INetworkEventRepository))
        {
            return (T)(object)NetworkEventRepositoryInstance;
        }

        if (requestedType == typeof(TestThreatFindingRepository) || requestedType == typeof(IThreatFindingRepository))
        {
            return (T)(object)ThreatFindingRepositoryInstance;
        }

        if (requestedType == typeof(TestAgentDecisionRepository) || requestedType == typeof(IAgentDecisionRepository))
        {
            return (T)(object)AgentDecisionRepositoryInstance;
        }

        return Services.GetRequiredService<T>();
    }

    public void Dispose() => Cluster.StopAllSilos();

    private sealed class TestSiloConfigurator : ISiloConfigurator
    {
        public void Configure(ISiloBuilder siloBuilder)
        {
            siloBuilder
                .UseLocalhostClustering()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "UbntSecPilot";
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton<TestOutboxRepository>(_ => OutboxRepositoryInstance);
                    services.AddSingleton<IOutboxRepository>(_ => OutboxRepositoryInstance);

                    services.AddSingleton<TestNetworkEventRepository>(_ => NetworkEventRepositoryInstance);
                    services.AddSingleton<INetworkEventRepository>(_ => NetworkEventRepositoryInstance);

                    services.AddSingleton<TestThreatFindingRepository>(_ => ThreatFindingRepositoryInstance);
                    services.AddSingleton<IThreatFindingRepository>(_ => ThreatFindingRepositoryInstance);

                    services.AddSingleton<TestAgentDecisionRepository>(_ => AgentDecisionRepositoryInstance);
                    services.AddSingleton<IAgentDecisionRepository>(_ => AgentDecisionRepositoryInstance);

                    services.AddSingleton<PreAnalysisService>();

                    services.AddLogging(logging =>
                    {
                        logging.AddConsole();
                        logging.SetMinimumLevel(LogLevel.Warning);
                    });

                    services.AddSingleton<IConfiguration>(_ => new ConfigurationBuilder()
                        .AddInMemoryCollection(new Dictionary<string, string?>
                        {
                            ["Agents:Use2PC"] = "true"
                        })
                        .Build());
                });
        }
    }

    public sealed class TestOutboxRepository : IOutboxRepository
    {
        private readonly ConcurrentDictionary<(string TxId, string ParticipantKey), OutboxRecord> _store = new();

        public IReadOnlyDictionary<(string TxId, string ParticipantKey), OutboxRecord> Snapshot() => _store;

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
                (_, rec) =>
                {
                    rec.Status = "committed";
                    return rec;
                });
            return Task.CompletedTask;
        }

        public Task MarkAbortedAsync(string txId, string participantKey)
        {
            _store.AddOrUpdate((txId, participantKey),
                _ => new OutboxRecord { TxId = txId, ParticipantKey = participantKey, Status = "aborted" },
                (_, rec) =>
                {
                    rec.Status = "aborted";
                    return rec;
                });
            return Task.CompletedTask;
        }

        public Task<OutboxRecord?> GetAsync(string txId, string participantKey)
        {
            _store.TryGetValue((txId, participantKey), out var record);
            return Task.FromResult(record);
        }

        public void Reset() => _store.Clear();
    }

    public sealed class TestNetworkEventRepository : INetworkEventRepository
    {
        private readonly ConcurrentDictionary<string, NetworkEvent> _store = new();

        public IReadOnlyDictionary<string, NetworkEvent> Snapshot => _store;

        public void Add(NetworkEvent networkEvent) => _store[networkEvent.EventId] = networkEvent;

        public Task<NetworkEvent> GetByIdAsync(string eventId)
            => Task.FromResult(_store.TryGetValue(eventId, out var ev) ? ev : null!);

        public Task<IEnumerable<NetworkEvent>> GetAllAsync(int limit = 100)
            => Task.FromResult<IEnumerable<NetworkEvent>>(_store.Values.Take(limit).ToList());

        public Task<IEnumerable<NetworkEvent>> GetUnprocessedEventsAsync(int limit = 10)
            => Task.FromResult<IEnumerable<NetworkEvent>>(_store.Values.Where(ev => ev.Status != EventStatus.Processed).Take(limit).ToList());

        public Task SaveAsync(NetworkEvent networkEvent)
        {
            _store[networkEvent.EventId] = networkEvent;
            return Task.CompletedTask;
        }

        public void Reset() => _store.Clear();
    }

    public sealed class TestThreatFindingRepository : IThreatFindingRepository
    {
        private readonly List<ThreatFinding> _store = new();

        public IReadOnlyCollection<ThreatFinding> Snapshot => _store.AsReadOnly();

        public Task SaveAsync(ThreatFinding threatFinding)
        {
            _store.Add(threatFinding);
            return Task.CompletedTask;
        }

        public void Reset() => _store.Clear();
    }

    public sealed class TestAgentDecisionRepository : IAgentDecisionRepository
    {
        private readonly List<AgentDecision> _store = new();

        public IReadOnlyCollection<AgentDecision> Snapshot => _store.AsReadOnly();

        public Task SaveAsync(AgentDecision agentDecision)
        {
            _store.Add(agentDecision);
            return Task.CompletedTask;
        }

        public void Reset() => _store.Clear();
    }

    private sealed class TestClientConfigurator : IClientBuilderConfigurator
    {
        public void Configure(IConfiguration configuration, IClientBuilder clientBuilder)
        {
            // Client configuration if needed
        }
    }
}

[CollectionDefinition("OrleansCluster")]
public sealed class OrleansClusterCollection : ICollectionFixture<OrleansTestClusterFixture>
{
}
