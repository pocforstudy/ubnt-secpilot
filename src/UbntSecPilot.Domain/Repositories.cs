using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UbntSecPilot.Domain.Models;

namespace UbntSecPilot.Domain.Repositories
{
    /// <summary>
    /// Repository interface for NetworkEvent operations.
    /// </summary>
    public interface INetworkEventRepository
    {
        Task<NetworkEvent> GetByIdAsync(string eventId);
        Task<IEnumerable<NetworkEvent>> GetAllAsync(int limit = 100);
        Task<IEnumerable<NetworkEvent>> GetBySourceAsync(string source, int limit = 100);
        Task<IEnumerable<NetworkEvent>> GetByTimeRangeAsync(DateTime startTime, DateTime endTime);
        Task<IEnumerable<NetworkEvent>> GetUnprocessedEventsAsync(int limit = 10);
        Task SaveAsync(NetworkEvent networkEvent);
        Task SaveManyAsync(IEnumerable<NetworkEvent> networkEvents);
        Task UpdateAsync(NetworkEvent networkEvent);
        Task DeleteAsync(string eventId);
    }

    /// <summary>
    /// Repository interface for ThreatFinding operations.
    /// </summary>
    public interface IThreatFindingRepository
    {
        Task<ThreatFinding> GetByIdAsync(string findingId);
        Task<IEnumerable<ThreatFinding>> GetAllAsync(int limit = 100);
        Task<IEnumerable<ThreatFinding>> GetBySeverityAsync(string severity, int limit = 100);
        Task<IEnumerable<ThreatFinding>> GetByTimeRangeAsync(DateTime startTime, DateTime endTime);
        Task SaveAsync(ThreatFinding threatFinding);
        Task SaveManyAsync(IEnumerable<ThreatFinding> threatFindings);
        Task UpdateAsync(ThreatFinding threatFinding);
        Task DeleteAsync(string findingId);
    }

    /// <summary>
    /// Repository interface for AgentDecision operations.
    /// </summary>
    public interface IAgentDecisionRepository
    {
        Task<AgentDecision> GetByIdAsync(string decisionId);
        Task<IEnumerable<AgentDecision>> GetAllAsync(int limit = 100);
        Task<IEnumerable<AgentDecision>> GetByTimeRangeAsync(DateTime startTime, DateTime endTime);
        Task SaveAsync(AgentDecision agentDecision);
        Task SaveManyAsync(IEnumerable<AgentDecision> agentDecisions);
        Task UpdateAsync(AgentDecision agentDecision);
        Task DeleteAsync(string decisionId);
    }

    /// <summary>
    /// Repository interface for thread analysis operations.
    /// </summary>
    public interface IThreadAnalysisRepository
    {
        Task<ThreadAnalysis> GetByIdAsync(string analysisId);
        Task<IEnumerable<ThreadAnalysis>> GetAllAsync(int limit = 100);
        Task SaveAsync(ThreadAnalysis threadAnalysis);
        Task UpdateAsync(ThreadAnalysis threadAnalysis);
        Task DeleteAsync(string analysisId);
    }
}
