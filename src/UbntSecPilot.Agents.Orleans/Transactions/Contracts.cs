using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Orleans.Serialization;
namespace UbntSecPilot.Agents.Orleans.Transactions
{
    [GenerateSerializer]
    public sealed record PrepareResult([
        property: Id(0)
    ] bool Ok, [
        property: Id(1)
    ] string? Error = null);

    [GenerateSerializer]
    public class TransactionPayload
    {
        [Id(0)]
        public string TransactionId { get; init; } = string.Empty;

        [Id(1)]
        public SerializableNetworkEvent? UpdatedEvent { get; init; }

        [Id(2)]
        public SerializableThreatFinding? Finding { get; init; }

        [Id(3)]
        public SerializableAgentDecision? Decision { get; init; }

        [Id(4)]
        public Dictionary<string, object>? Metadata { get; init; }

        public TransactionPayload()
        {
        }

        public Domain.Models.NetworkEvent? GetUpdatedEvent()
            => UpdatedEvent?.ToDomain();

        public Domain.Models.ThreatFinding? GetFinding()
            => Finding?.ToDomain();

        public Domain.Models.AgentDecision? GetDecision()
            => Decision?.ToDomain();
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
