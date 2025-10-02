using System.Collections.Generic;
using System.Threading.Tasks;
using UbntSecPilot.Domain.Models;

namespace UbntSecPilot.Application.Services
{
    /// <summary>
    /// Interface for thread analysis operations
    /// </summary>
    public interface IThreadAnalysisService
    {
        /// <summary>
        /// Analyzes a thread for potential security issues
        /// </summary>
        /// <param name="threadId">The thread identifier</param>
        /// <param name="isIoc">Whether this is an indicator of compromise</param>
        /// <param name="severity">The severity level</param>
        /// <param name="reason">The reason for analysis</param>
        /// <param name="indicators">List of indicators found</param>
        /// <returns>Thread analysis result</returns>
        Task<ThreadAnalysis> AnalyzeThreadAsync(string threadId, bool isIoc, string severity, string reason, List<string> indicators);
    }
}
