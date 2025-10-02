using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UbntSecPilot.Domain.Models;
using UbntSecPilot.Domain.Repositories;
using UbntSecPilot.Domain.ValueObjects;

namespace UbntSecPilot.Infrastructure.Data.SqlServer
{
    /// <summary>
    /// Entity Framework Core DbContext for SQL Server
    /// </summary>
    public class SecurityDbContext : DbContext
    {
        public DbSet<NetworkEvent> NetworkEvents { get; set; }
        public DbSet<ThreatFinding> ThreatFindings { get; set; }
        public DbSet<AgentDecision> AgentDecisions { get; set; }
        public DbSet<ThreadAnalysis> ThreadAnalyses { get; set; }

        public SecurityDbContext(DbContextOptions<SecurityDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure NetworkEvent
            modelBuilder.Entity<NetworkEvent>(entity =>
            {
                entity.HasKey(e => e.EventId);
                entity.Property(e => e.EventId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Source).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Payload).HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions)null)
                );
                entity.Property(e => e.OccurredAt).IsRequired();
                entity.Property(e => e.Status).HasConversion<string>();
                entity.Property(e => e.Priority).HasConversion<string>();
            });

            // Configure ThreatFinding
            modelBuilder.Entity<ThreatFinding>(entity =>
            {
                entity.HasKey(f => f.Id);
                entity.Property(f => f.EventId).IsRequired().HasMaxLength(100);
                entity.Property(f => f.Severity).IsRequired().HasMaxLength(50);
                entity.Property(f => f.Summary).IsRequired().HasMaxLength(1000);
                entity.Property(f => f.Metadata).HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions)null)
                );
                entity.Property(f => f.Status).HasConversion<string>();
                entity.Property(f => f.AssignedTo).HasMaxLength(100);
            });

            // Configure AgentDecision
            modelBuilder.Entity<AgentDecision>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.Action).IsRequired().HasMaxLength(200);
                entity.Property(d => d.Reason).IsRequired().HasMaxLength(1000);
                entity.Property(d => d.Metadata).HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions)null)
                );
                entity.Property(d => d.Status).HasConversion<string>();
                entity.Property(d => d.ExecutedBy).HasMaxLength(100);
            });

            // Configure ThreadAnalysis
            modelBuilder.Entity<ThreadAnalysis>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.ThreadId).IsRequired().HasMaxLength(100);
                entity.Property(a => a.Severity).IsRequired().HasMaxLength(50);
                entity.Property(a => a.Reason).IsRequired().HasMaxLength(1000);
                entity.Property(a => a.Indicators).HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions)null)
                );
                entity.Property(a => a.Metadata).HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions)null)
                );
                entity.Property(a => a.Status).HasConversion<string>();
            });
        }
    }

    /// <summary>
    /// SQL Server implementation of event repository using Entity Framework
    /// </summary>
    public class SqlServerEventRepository : INetworkEventRepository
    {
        private readonly SecurityDbContext _context;

        public SqlServerEventRepository(SecurityDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<NetworkEvent> GetByIdAsync(string eventId)
        {
            return await _context.NetworkEvents
                .FirstOrDefaultAsync(e => e.EventId == eventId);
        }

        public async Task<IEnumerable<NetworkEvent>> GetAllAsync(int limit = 100)
        {
            return await _context.NetworkEvents
                .OrderByDescending(e => e.OccurredAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<NetworkEvent>> GetBySourceAsync(string source, int limit = 100)
        {
            return await _context.NetworkEvents
                .Where(e => e.Source == source)
                .OrderByDescending(e => e.OccurredAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<NetworkEvent>> GetByTimeRangeAsync(DateTime startTime, DateTime endTime)
        {
            return await _context.NetworkEvents
                .Where(e => e.OccurredAt >= startTime && e.OccurredAt <= endTime)
                .OrderByDescending(e => e.OccurredAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<NetworkEvent>> GetUnprocessedEventsAsync(int limit = 10)
        {
            return await _context.NetworkEvents
                .Where(e => e.Status == EventStatus.New)
                .OrderBy(e => e.OccurredAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task SaveAsync(NetworkEvent networkEvent)
        {
            var existing = await _context.NetworkEvents
                .FirstOrDefaultAsync(e => e.EventId == networkEvent.EventId);

            if (existing == null)
            {
                await _context.NetworkEvents.AddAsync(networkEvent);
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(networkEvent);
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(NetworkEvent networkEvent)
        {
            await SaveAsync(networkEvent);
        }

        public async Task DeleteAsync(string eventId)
        {
            var networkEvent = await GetByIdAsync(eventId);
            if (networkEvent != null)
            {
                _context.NetworkEvents.Remove(networkEvent);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SaveManyAsync(IEnumerable<NetworkEvent> networkEvents)
        {
            await _context.NetworkEvents.AddRangeAsync(networkEvents);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// SQL Server implementation of finding repository using Entity Framework
    /// </summary>
    public class SqlServerFindingRepository : IThreatFindingRepository
    {
        private readonly SecurityDbContext _context;

        public SqlServerFindingRepository(SecurityDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<ThreatFinding> GetByIdAsync(string findingId)
        {
            return await _context.ThreatFindings
                .FirstOrDefaultAsync(f => f.Id == findingId);
        }

        public async Task<IEnumerable<ThreatFinding>> GetAllAsync(int limit = 100)
        {
            return await _context.ThreatFindings
                .OrderByDescending(f => f.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<ThreatFinding>> GetBySeverityAsync(string severity, int limit = 100)
        {
            return await _context.ThreatFindings
                .Where(f => f.Severity == severity)
                .OrderByDescending(f => f.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<ThreatFinding>> GetByTimeRangeAsync(DateTime startTime, DateTime endTime)
        {
            return await _context.ThreatFindings
                .Where(f => f.CreatedAt >= startTime && f.CreatedAt <= endTime)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task SaveAsync(ThreatFinding threatFinding)
        {
            var existing = await _context.ThreatFindings
                .FirstOrDefaultAsync(f => f.Id == threatFinding.Id);

            if (existing == null)
            {
                await _context.ThreatFindings.AddAsync(threatFinding);
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(threatFinding);
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ThreatFinding threatFinding)
        {
            await SaveAsync(threatFinding);
        }

        public async Task DeleteAsync(string findingId)
        {
            var threatFinding = await GetByIdAsync(findingId);
            if (threatFinding != null)
            {
                _context.ThreatFindings.Remove(threatFinding);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SaveManyAsync(IEnumerable<ThreatFinding> threatFindings)
        {
            await _context.ThreatFindings.AddRangeAsync(threatFindings);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// SQL Server implementation of decision repository using Entity Framework
    /// </summary>
    public class SqlServerDecisionRepository : IAgentDecisionRepository
    {
        private readonly SecurityDbContext _context;

        public SqlServerDecisionRepository(SecurityDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<AgentDecision> GetByIdAsync(string decisionId)
        {
            return await _context.AgentDecisions
                .FirstOrDefaultAsync(d => d.Id == decisionId);
        }

        public async Task<IEnumerable<AgentDecision>> GetAllAsync(int limit = 100)
        {
            return await _context.AgentDecisions
                .OrderByDescending(d => d.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<AgentDecision>> GetByTimeRangeAsync(DateTime startTime, DateTime endTime)
        {
            return await _context.AgentDecisions
                .Where(d => d.CreatedAt >= startTime && d.CreatedAt <= endTime)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task SaveAsync(AgentDecision agentDecision)
        {
            var existing = await _context.AgentDecisions
                .FirstOrDefaultAsync(d => d.Id == agentDecision.Id);

            if (existing == null)
            {
                await _context.AgentDecisions.AddAsync(agentDecision);
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(agentDecision);
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(AgentDecision agentDecision)
        {
            await SaveAsync(agentDecision);
        }

        public async Task DeleteAsync(string decisionId)
        {
            var agentDecision = await GetByIdAsync(decisionId);
            if (agentDecision != null)
            {
                _context.AgentDecisions.Remove(agentDecision);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SaveManyAsync(IEnumerable<AgentDecision> agentDecisions)
        {
            await _context.AgentDecisions.AddRangeAsync(agentDecisions);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// SQL Server implementation of thread analysis repository using Entity Framework
    /// </summary>
    public class SqlServerThreadAnalysisRepository : IThreadAnalysisRepository
    {
        private readonly SecurityDbContext _context;

        public SqlServerThreadAnalysisRepository(SecurityDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<ThreadAnalysis> GetByIdAsync(string analysisId)
        {
            return await _context.ThreadAnalyses
                .FirstOrDefaultAsync(a => a.Id == analysisId);
        }

        public async Task<IEnumerable<ThreadAnalysis>> GetAllAsync(int limit = 100)
        {
            return await _context.ThreadAnalyses
                .OrderByDescending(a => a.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task SaveAsync(ThreadAnalysis threadAnalysis)
        {
            var existing = await _context.ThreadAnalyses
                .FirstOrDefaultAsync(a => a.Id == threadAnalysis.Id);

            if (existing == null)
            {
                await _context.ThreadAnalyses.AddAsync(threadAnalysis);
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(threadAnalysis);
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ThreadAnalysis threadAnalysis)
        {
            await SaveAsync(threadAnalysis);
        }

        public async Task DeleteAsync(string analysisId)
        {
            var threadAnalysis = await GetByIdAsync(analysisId);
            if (threadAnalysis != null)
            {
                _context.ThreadAnalyses.Remove(threadAnalysis);
                await _context.SaveChangesAsync();
            }
        }
    }
}
