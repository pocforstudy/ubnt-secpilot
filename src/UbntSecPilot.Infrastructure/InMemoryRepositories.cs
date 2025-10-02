using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UbntSecPilot.Domain.Models;
using UbntSecPilot.Domain.Repositories;

namespace UbntSecPilot.Infrastructure.Data.InMemory
{
    /// <summary>
    /// In-memory implementation of finding repository for testing
    /// </summary>
    public class InMemoryFindingRepository : IThreatFindingRepository
    {
        private readonly List<ThreatFinding> _findings = new();
        private readonly object _lock = new();

        public async Task<ThreatFinding> GetByIdAsync(string findingId)
        {
            await Task.Delay(1); // Simulate async operation
            lock (_lock)
            {
                return _findings.FirstOrDefault(f => f.Id == findingId);
            }
        }

        public async Task<IEnumerable<ThreatFinding>> GetAllAsync(int limit = 100)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                return _findings.OrderByDescending(f => f.CreatedAt).Take(limit);
            }
        }

        public async Task<IEnumerable<ThreatFinding>> GetBySeverityAsync(string severity, int limit = 100)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                return _findings.Where(f => f.Severity == severity)
                    .OrderByDescending(f => f.CreatedAt)
                    .Take(limit);
            }
        }

        public async Task<IEnumerable<ThreatFinding>> GetByTimeRangeAsync(DateTime startTime, DateTime endTime)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                return _findings.Where(f => f.CreatedAt >= startTime && f.CreatedAt <= endTime)
                    .OrderByDescending(f => f.CreatedAt);
            }
        }

        public async Task SaveAsync(ThreatFinding threatFinding)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                var existing = _findings.FirstOrDefault(f => f.Id == threatFinding.Id);
                if (existing != null)
                {
                    _findings.Remove(existing);
                }
                _findings.Add(threatFinding);
            }
        }

        public async Task SaveManyAsync(IEnumerable<ThreatFinding> threatFindings)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                foreach (var threatFinding in threatFindings)
                {
                    var existing = _findings.FirstOrDefault(f => f.Id == threatFinding.Id);
                    if (existing != null)
                    {
                        _findings.Remove(existing);
                    }
                    _findings.Add(threatFinding);
                }
            }
        }

        public async Task UpdateAsync(ThreatFinding threatFinding)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                var existing = _findings.FirstOrDefault(f => f.Id == threatFinding.Id);
                if (existing != null)
                {
                    _findings.Remove(existing);
                }
                _findings.Add(threatFinding);
            }
        }

        public async Task DeleteAsync(string findingId)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                var threatFinding = _findings.FirstOrDefault(f => f.Id == findingId);
                if (threatFinding != null)
                {
                    _findings.Remove(threatFinding);
                }
            }
        }
    }

    /// <summary>
    /// In-memory implementation of thread analysis repository for testing
    /// </summary>
    public class InMemoryThreadAnalysisRepository : IThreadAnalysisRepository
    {
        private readonly List<ThreadAnalysis> _analyses = new();
        private readonly object _lock = new();

        public async Task<ThreadAnalysis> GetByIdAsync(string analysisId)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                return _analyses.FirstOrDefault(a => a.Id == analysisId);
            }
        }

        public async Task<IEnumerable<ThreadAnalysis>> GetAllAsync(int limit = 100)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                return _analyses.OrderByDescending(a => a.CreatedAt).Take(limit);
            }
        }

        public async Task SaveAsync(ThreadAnalysis threadAnalysis)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                var existing = _analyses.FirstOrDefault(a => a.Id == threadAnalysis.Id);
                if (existing != null)
                {
                    _analyses.Remove(existing);
                }
                _analyses.Add(threadAnalysis);
            }
        }

        public async Task UpdateAsync(ThreadAnalysis threadAnalysis)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                var existing = _analyses.FirstOrDefault(a => a.Id == threadAnalysis.Id);
                if (existing != null)
                {
                    _analyses.Remove(existing);
                }
                _analyses.Add(threadAnalysis);
            }
        }

        public async Task DeleteAsync(string analysisId)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                var threadAnalysis = _analyses.FirstOrDefault(a => a.Id == analysisId);
                if (threadAnalysis != null)
                {
                    _analyses.Remove(threadAnalysis);
                }
            }
        }
    }
}
