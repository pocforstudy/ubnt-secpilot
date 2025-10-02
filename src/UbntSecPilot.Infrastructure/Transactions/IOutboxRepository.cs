using System.Collections.Generic;
using System.Threading.Tasks;

namespace UbntSecPilot.Infrastructure.Transactions
{
    public sealed class OutboxRecord
    {
        public string TxId { get; init; } = string.Empty;
        public string ParticipantKey { get; init; } = string.Empty;
        public string Status { get; set; } = "prepared"; // prepared | committed | aborted
        public Dictionary<string, object> Data { get; init; } = new();
    }

    public interface IOutboxRepository
    {
        Task SavePreparedAsync(OutboxRecord record);
        Task MarkCommittedAsync(string txId, string participantKey);
        Task MarkAbortedAsync(string txId, string participantKey);
        Task<OutboxRecord?> GetAsync(string txId, string participantKey);
    }
}
