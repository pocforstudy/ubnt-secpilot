using System.Threading;
using System.Threading.Tasks;

namespace UbntSecPilot.Agents
{
    public interface IAgent
    {
        string Name { get; }
        Task<AgentResult> RunAsync(CancellationToken cancellationToken = default);
    }
}
