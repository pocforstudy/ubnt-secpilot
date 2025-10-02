using System.Threading.Tasks;
using UbntSecPilot.Domain.Models;

namespace UbntSecPilot.Application.Services
{
    /// <summary>
    /// Interface for threat analysis operations
    /// </summary>
    public interface IThreatAnalysisService
    {
        /// <summary>
        /// Analyzes a network event for potential threats
        /// </summary>
        /// <param name="networkEvent">The network event to analyze</param>
        /// <returns>Threat finding if threats detected, null otherwise</returns>
        Task<ThreatFinding?> AnalyzeNetworkEventAsync(NetworkEvent networkEvent);
        
        /// <summary>
        /// Runs threat enrichment process
        /// </summary>
        /// <returns>Agent decision with enrichment results</returns>
        Task<AgentDecision> RunThreatEnrichmentAsync();
    }
}
