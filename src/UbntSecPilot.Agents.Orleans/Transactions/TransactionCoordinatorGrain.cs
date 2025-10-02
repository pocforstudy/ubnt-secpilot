using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;

namespace UbntSecPilot.Agents.Orleans.Transactions
{
    public sealed class TransactionCoordinatorGrain : Grain, ITransactionCoordinatorGrain
    {
        private readonly ILogger<TransactionCoordinatorGrain> _logger;

        public TransactionCoordinatorGrain(ILogger<TransactionCoordinatorGrain> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> RunTwoPhaseCommitAsync(TransactionPayload payload, IReadOnlyList<string> participants)
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));
            if (participants == null || participants.Count == 0) return true;

            var txId = string.IsNullOrWhiteSpace(payload.TransactionId) ? Guid.NewGuid().ToString("N") : payload.TransactionId;
            var prepared = new List<string>();

            try
            {
                // Prepare phase
                foreach (var key in participants)
                {
                    var participant = GrainFactory.GetGrain<IParticipantGrain>(key);
                    var res = await participant.PrepareAsync(txId, payload).ConfigureAwait(false);
                    if (!res.Ok)
                    {
                        _logger.LogWarning("Prepare failed for {Key}: {Error}", key, res.Error);
                        // Abort previously prepared
                        foreach (var p in prepared)
                        {
                            try { await GrainFactory.GetGrain<IParticipantGrain>(p).AbortAsync(txId, payload).ConfigureAwait(false); }
                            catch (Exception ex) { _logger.LogError(ex, "Abort failed for {Key}", p); }
                        }
                        return false;
                    }
                    prepared.Add(key);
                }

                // Commit phase
                foreach (var key in participants)
                {
                    await GrainFactory.GetGrain<IParticipantGrain>(key).CommitAsync(txId, payload).ConfigureAwait(false);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Coordinator exception, aborting tx {TxId}", txId);
                foreach (var p in prepared)
                {
                    try { await GrainFactory.GetGrain<IParticipantGrain>(p).AbortAsync(txId, payload).ConfigureAwait(false); }
                    catch (Exception aex) { _logger.LogError(aex, "Abort failed for {Key}", p); }
                }
                return false;
            }
        }
    }
}
