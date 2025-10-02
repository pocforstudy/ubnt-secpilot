using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using UbntSecPilot.Domain.Models;

namespace UbntSecPilot.Agents.Orleans.Transactions
{
    public record PrepareResult(bool Ok, string? Error = null);

    public class TransactionPayload
    {
        public string TransactionId { get; init; } = string.Empty;
        public NetworkEvent? UpdatedEvent { get; init; }
        public ThreatFinding? Finding { get; init; }
        public AgentDecision? Decision { get; init; }
        public Dictionary<string, object>? Metadata { get; init; }
    }

    public interface IParticipantGrain : IGrainWithStringKey
    {
        Task<PrepareResult> PrepareAsync(string txId, TransactionPayload payload);
        Task CommitAsync(string txId, TransactionPayload payload);
        Task AbortAsync(string txId, TransactionPayload payload);
    }

    public interface ITransactionCoordinatorGrain : IGrainWithStringKey
    {
        Task<bool> RunTwoPhaseCommitAsync(TransactionPayload payload, IReadOnlyList<string> participants);
    }
}
