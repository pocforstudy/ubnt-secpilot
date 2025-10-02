using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using StackExchange.Redis;
using UbntSecPilot.Domain.Models;
using UbntSecPilot.Domain.Repositories;

namespace UbntSecPilot.Infrastructure.Data.Redis
{
    /// <summary>
    /// Redis implementation of finding repository for caching
    /// </summary>
    public class RedisFindingRepository : IThreatFindingRepository
    {
        private readonly IDatabase _database;
        private readonly string _findingsKey = "findings";
        private readonly string _findingPrefix = "finding:";

        public RedisFindingRepository(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }

        public async Task<ThreatFinding> GetByIdAsync(string findingId)
        {
            var key = $"{_findingPrefix}{findingId}";
            var findingJson = await _database.StringGetAsync(key);

            if (findingJson.IsNullOrEmpty)
                return null;

            return JsonSerializer.Deserialize<ThreatFinding>(findingJson);
        }

        public async Task<IEnumerable<ThreatFinding>> GetAllAsync(int limit = 100)
        {
            var findingIds = await _database.SetMembersAsync(_findingsKey);
            var findings = new List<ThreatFinding>();

            foreach (var findingId in findingIds.Take(limit))
            {
                var threatFinding = await GetByIdAsync(findingId.ToString());
                if (threatFinding != null)
                    findings.Add(threatFinding);
            }

            return findings.OrderByDescending(f => f.CreatedAt);
        }

        public async Task<IEnumerable<ThreatFinding>> GetBySeverityAsync(string severity, int limit = 100)
        {
            var findingIds = await _database.SetMembersAsync(_findingsKey);
            var findings = new List<ThreatFinding>();

            foreach (var findingId in findingIds)
            {
                var threatFinding = await GetByIdAsync(findingId.ToString());
                if (threatFinding != null && threatFinding.Severity == severity)
                    findings.Add(threatFinding);
            }

            return findings.OrderByDescending(f => f.CreatedAt).Take(limit);
        }

        public async Task<IEnumerable<ThreatFinding>> GetByTimeRangeAsync(DateTime startTime, DateTime endTime)
        {
            var findingIds = await _database.SetMembersAsync(_findingsKey);
            var findings = new List<ThreatFinding>();

            foreach (var findingId in findingIds)
            {
                var threatFinding = await GetByIdAsync(findingId.ToString());
                if (threatFinding != null && threatFinding.CreatedAt >= startTime && threatFinding.CreatedAt <= endTime)
                    findings.Add(threatFinding);
            }

            return findings.OrderByDescending(f => f.CreatedAt);
        }

        public async Task SaveAsync(ThreatFinding threatFinding)
        {
            var findingJson = JsonSerializer.Serialize(threatFinding);
            var key = $"{_findingPrefix}{threatFinding.Id}";

            await _database.StringSetAsync(key, findingJson);
            await _database.SetAddAsync(_findingsKey, threatFinding.Id);

            // Set expiration
            await _database.KeyExpireAsync(key, TimeSpan.FromDays(30));
        }

        public async Task SaveManyAsync(IEnumerable<ThreatFinding> threatFindings)
        {
            foreach (var threatFinding in threatFindings)
            {
                var findingJson = JsonSerializer.Serialize(threatFinding);
                var key = $"{_findingPrefix}{threatFinding.Id}";

                await _database.StringSetAsync(key, findingJson);
                await _database.SetAddAsync(_findingsKey, threatFinding.Id);

                // Set expiration
                await _database.KeyExpireAsync(key, TimeSpan.FromDays(30));
            }
        }

        public async Task UpdateAsync(ThreatFinding threatFinding)
        {
            await SaveAsync(threatFinding);
        }

        public async Task DeleteAsync(string findingId)
        {
            var key = $"{_findingPrefix}{findingId}";
            await _database.KeyDeleteAsync(key);
            await _database.SetRemoveAsync(_findingsKey, findingId);
        }
    }
}
