using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using UbntSecPilot.Domain.Models;
using UbntSecPilot.Domain.Repositories;
using UbntSecPilot.Domain.ValueObjects;

namespace UbntSecPilot.Infrastructure.Repositories
{
    /// <summary>
    /// MongoDB implementation of event repository
    /// </summary>
    public class MongoEventRepository : INetworkEventRepository
    {
        private readonly IMongoCollection<NetworkEvent> _eventsCollection;

        public MongoEventRepository(IMongoDatabase database)
        {
            _eventsCollection = database.GetCollection<NetworkEvent>("events");
        }

        public async Task<NetworkEvent> GetByIdAsync(string eventId)
        {
            var filter = Builders<NetworkEvent>.Filter.Eq(e => e.EventId, eventId);
            return await _eventsCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<NetworkEvent>> GetAllAsync(int limit = 100)
        {
            var events = await _eventsCollection
                .Find(_ => true)
                .SortByDescending(e => e.OccurredAt)
                .Limit(limit)
                .ToListAsync();

            return events;
        }

        public async Task<IEnumerable<NetworkEvent>> GetBySourceAsync(string source, int limit = 100)
        {
            var filter = Builders<NetworkEvent>.Filter.Eq(e => e.Source, source);
            var events = await _eventsCollection
                .Find(filter)
                .SortByDescending(e => e.OccurredAt)
                .Limit(limit)
                .ToListAsync();

            return events;
        }

        public async Task<IEnumerable<NetworkEvent>> GetByTimeRangeAsync(DateTime startTime, DateTime endTime)
        {
            var filter = Builders<NetworkEvent>.Filter.And(
                Builders<NetworkEvent>.Filter.Gte(e => e.OccurredAt, startTime),
                Builders<NetworkEvent>.Filter.Lte(e => e.OccurredAt, endTime)
            );

            return await _eventsCollection
                .Find(filter)
                .SortByDescending(e => e.OccurredAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<NetworkEvent>> GetUnprocessedEventsAsync(int limit = 10)
        {
            var filter = Builders<NetworkEvent>.Filter.Eq(e => e.Status, EventStatus.New);
            var events = await _eventsCollection
                .Find(filter)
                .SortBy(e => e.OccurredAt)
                .Limit(limit)
                .ToListAsync();

            return events;
        }

        public async Task SaveAsync(NetworkEvent networkEvent)
        {
            var filter = Builders<NetworkEvent>.Filter.Eq(e => e.EventId, networkEvent.EventId);
            var options = new ReplaceOptions { IsUpsert = true };

            await _eventsCollection.ReplaceOneAsync(filter, networkEvent, options);
        }

        public async Task UpdateAsync(NetworkEvent networkEvent)
        {
            await SaveAsync(networkEvent);
        }

        public async Task DeleteAsync(string eventId)
        {
            var filter = Builders<NetworkEvent>.Filter.Eq(e => e.EventId, eventId);
            await _eventsCollection.DeleteOneAsync(filter);
        }

        public async Task SaveManyAsync(IEnumerable<NetworkEvent> networkEvents)
        {
            var writes = networkEvents.Select(e => new ReplaceOneModel<NetworkEvent>(Builders<NetworkEvent>.Filter.Eq(x => x.EventId, e.EventId), e) { IsUpsert = true });
            await _eventsCollection.BulkWriteAsync(writes);
        }
    }

    /// <summary>
    /// MongoDB implementation of finding repository
    /// </summary>
    public class MongoFindingRepository : IThreatFindingRepository
    {
        private readonly IMongoCollection<ThreatFinding> _findingsCollection;

        public MongoFindingRepository(IMongoDatabase database)
        {
            _findingsCollection = database.GetCollection<ThreatFinding>("findings");
        }

        public async Task<ThreatFinding> GetByIdAsync(string findingId)
        {
            var filter = Builders<ThreatFinding>.Filter.Eq(f => f.Id, findingId);
            return await _findingsCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ThreatFinding>> GetAllAsync(int limit = 100)
        {
            var findings = await _findingsCollection
                .Find(_ => true)
                .SortByDescending(f => f.CreatedAt)
                .Limit(limit)
                .ToListAsync();

            return findings;
        }

        public async Task<IEnumerable<ThreatFinding>> GetBySeverityAsync(string severity, int limit = 100)
        {
            var filter = Builders<ThreatFinding>.Filter.Eq(f => f.Severity, severity);
            var findings = await _findingsCollection
                .Find(filter)
                .SortByDescending(f => f.CreatedAt)
                .Limit(limit)
                .ToListAsync();

            return findings;
        }

        public async Task<IEnumerable<ThreatFinding>> GetByTimeRangeAsync(DateTime startTime, DateTime endTime)
        {
            var filter = Builders<ThreatFinding>.Filter.And(
                Builders<ThreatFinding>.Filter.Gte(f => f.CreatedAt, startTime),
                Builders<ThreatFinding>.Filter.Lte(f => f.CreatedAt, endTime)
            );

            return await _findingsCollection
                .Find(filter)
                .SortByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task SaveAsync(ThreatFinding threatFinding)
        {
            var filter = Builders<ThreatFinding>.Filter.Eq(f => f.Id, threatFinding.Id);
            var options = new ReplaceOptions { IsUpsert = true };

            await _findingsCollection.ReplaceOneAsync(filter, threatFinding, options);
        }

        public async Task UpdateAsync(ThreatFinding threatFinding)
        {
            await SaveAsync(threatFinding);
        }

        public async Task DeleteAsync(string findingId)
        {
            var filter = Builders<ThreatFinding>.Filter.Eq(f => f.Id, findingId);
            await _findingsCollection.DeleteOneAsync(filter);
        }

        public async Task SaveManyAsync(IEnumerable<ThreatFinding> threatFindings)
        {
            var writes = threatFindings.Select(f => new ReplaceOneModel<ThreatFinding>(Builders<ThreatFinding>.Filter.Eq(x => x.Id, f.Id), f) { IsUpsert = true });
            await _findingsCollection.BulkWriteAsync(writes);
        }
    }

    /// <summary>
    /// MongoDB implementation of decision repository
    /// </summary>
    public class MongoDecisionRepository : IAgentDecisionRepository
    {
        private readonly IMongoCollection<AgentDecision> _decisionsCollection;

        public MongoDecisionRepository(IMongoDatabase database)
        {
            _decisionsCollection = database.GetCollection<AgentDecision>("decisions");
        }

        public async Task<AgentDecision> GetByIdAsync(string decisionId)
        {
            var filter = Builders<AgentDecision>.Filter.Eq(d => d.Id, decisionId);
            return await _decisionsCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<AgentDecision>> GetAllAsync(int limit = 100)
        {
            var decisions = await _decisionsCollection
                .Find(_ => true)
                .SortByDescending(d => d.CreatedAt)
                .Limit(limit)
                .ToListAsync();

            return decisions;
        }

        public async Task<IEnumerable<AgentDecision>> GetByTimeRangeAsync(DateTime startTime, DateTime endTime)
        {
            var filter = Builders<AgentDecision>.Filter.And(
                Builders<AgentDecision>.Filter.Gte(d => d.CreatedAt, startTime),
                Builders<AgentDecision>.Filter.Lte(d => d.CreatedAt, endTime)
            );

            return await _decisionsCollection
                .Find(filter)
                .SortByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task SaveAsync(AgentDecision agentDecision)
        {
            var filter = Builders<AgentDecision>.Filter.Eq(d => d.Id, agentDecision.Id);
            var options = new ReplaceOptions { IsUpsert = true };

            await _decisionsCollection.ReplaceOneAsync(filter, agentDecision, options);
        }

        public async Task UpdateAsync(AgentDecision agentDecision)
        {
            await SaveAsync(agentDecision);
        }

        public async Task DeleteAsync(string decisionId)
        {
            var filter = Builders<AgentDecision>.Filter.Eq(d => d.Id, decisionId);
            await _decisionsCollection.DeleteOneAsync(filter);
        }

        public async Task SaveManyAsync(IEnumerable<AgentDecision> agentDecisions)
        {
            var writes = agentDecisions.Select(d => new ReplaceOneModel<AgentDecision>(Builders<AgentDecision>.Filter.Eq(x => x.Id, d.Id), d) { IsUpsert = true });
            await _decisionsCollection.BulkWriteAsync(writes);
        }
    }

    /// <summary>
    /// MongoDB implementation of thread analysis repository
    /// </summary>
    public class MongoThreadAnalysisRepository : IThreadAnalysisRepository
    {
        private readonly IMongoCollection<ThreadAnalysis> _analysesCollection;

        public MongoThreadAnalysisRepository(IMongoDatabase database)
        {
            _analysesCollection = database.GetCollection<ThreadAnalysis>("thread_analyses");
        }

        public async Task<ThreadAnalysis> GetByIdAsync(string analysisId)
        {
            var filter = Builders<ThreadAnalysis>.Filter.Eq(a => a.Id, analysisId);
            return await _analysesCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ThreadAnalysis>> GetAllAsync(int limit = 100)
        {
            var analyses = await _analysesCollection
                .Find(_ => true)
                .SortByDescending(a => a.CreatedAt)
                .Limit(limit)
                .ToListAsync();

            return analyses;
        }

        public async Task SaveAsync(ThreadAnalysis threadAnalysis)
        {
            var filter = Builders<ThreadAnalysis>.Filter.Eq(a => a.Id, threadAnalysis.Id);
            var options = new ReplaceOptions { IsUpsert = true };

            await _analysesCollection.ReplaceOneAsync(filter, threadAnalysis, options);
        }

        public async Task UpdateAsync(ThreadAnalysis threadAnalysis)
        {
            await SaveAsync(threadAnalysis);
        }

        public async Task DeleteAsync(string analysisId)
        {
            var filter = Builders<ThreadAnalysis>.Filter.Eq(a => a.Id, analysisId);
            await _analysesCollection.DeleteOneAsync(filter);
        }
    }
}
