using System.Threading;
using System.Threading.Tasks;

namespace UbntSecPilot.Agents.Orleans
{
    public interface IAgentGrain : IGrainWithStringKey
    {
        Task<AgentResult> RunAsync(CancellationToken cancellationToken = default);
        Task<bool> IsRunning();
        Task<string> GetStatus();
    }
}
