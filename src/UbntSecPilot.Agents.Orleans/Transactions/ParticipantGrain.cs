using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using UbntSecPilot.Agents.Orleans.Transactions;
using UbntSecPilot.Domain.Models;
using UbntSecPilot.Domain.Repositories;
using UbntSecPilot.Infrastructure.Transactions;

namespace UbntSecPilot.Agents.Orleans.Transactions
{
    /// <summary>
    /// Generic participant that routes operations based on its key prefix:
    /// event:{id} | finding:{id} | decision:{id}
    /// </summary>
    public sealed class ParticipantGrain : Grain, IParticipantGrain
    {
        private readonly ILogger<ParticipantGrain> _logger;
        private readonly IOutboxRepository _outbox;
        private readonly INetworkEventRepository _events;
        private readonly IThreatFindingRepository _findings;
        private readonly IAgentDecisionRepository _decisions;

        public ParticipantGrain(
            ILogger<ParticipantGrain> logger,
            IOutboxRepository outbox,
            INetworkEventRepository events,
            IThreatFindingRepository findings,
            IAgentDecisionRepository decisions)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _outbox = outbox ?? throw new ArgumentNullException(nameof(outbox));
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _findings = findings ?? throw new ArgumentNullException(nameof(findings));
            _decisions = decisions ?? throw new ArgumentNullException(nameof(decisions));
        }

        private (string kind, string id) ParseKey()
        {
            var key = this.GetPrimaryKeyString();
            var idx = key.IndexOf(':');
            if (idx <= 0) return (key, string.Empty);
            return (key.Substring(0, idx), key.Substring(idx + 1));
        }

        public async Task<PrepareResult> PrepareAsync(string txId, TransactionPayload payload)
        {
            var (kind, id) = ParseKey();
            var data = new Dictionary<string, object>();
            try
            {
                switch (kind)
                {
                    case "event":
                        if (payload.UpdatedEvent is NetworkEvent nev && nev.EventId == id)
                        {
                            data["eventId"] = nev.EventId;
                            data["source"] = nev.Source;
                        }
                        break;
                    case "finding":
                        if (payload.Finding is ThreatFinding tf)
                        {
                            data["finding_reason"] = tf.Summary;
                            data["eventId"] = tf.EventId;
                        }
                        break;
                    case "decision":
                        if (payload.Decision is AgentDecision ad)
                        {
                            data["decision_action"] = ad.Action;
                            data["decision_reason"] = ad.Reason;
                        }
                        break;
                }

                await _outbox.SavePreparedAsync(new OutboxRecord
                {
                    TxId = txId,
                    ParticipantKey = this.GetPrimaryKeyString(),
                    Data = data
                }).ConfigureAwait(false);
                return new PrepareResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Prepare failed for {Key}", this.GetPrimaryKeyString());
                return new PrepareResult(false, ex.Message);
            }
        }

        public async Task CommitAsync(string txId, TransactionPayload payload)
        {
            var (kind, id) = ParseKey();
            switch (kind)
            {
                case "event":
                    if (payload.UpdatedEvent is NetworkEvent ev)
                    {
                        await _events.SaveAsync(ev).ConfigureAwait(false);
                    }
                    break;
                case "finding":
                    if (payload.Finding is ThreatFinding f)
                    {
                        await _findings.SaveAsync(f).ConfigureAwait(false);
                    }
                    break;
                case "decision":
                    if (payload.Decision is AgentDecision d)
                    {
                        await _decisions.SaveAsync(d).ConfigureAwait(false);
                    }
                    break;
            }
            await _outbox.MarkCommittedAsync(txId, this.GetPrimaryKeyString()).ConfigureAwait(false);
        }

        public async Task AbortAsync(string txId, TransactionPayload payload)
        {
            await _outbox.MarkAbortedAsync(txId, this.GetPrimaryKeyString()).ConfigureAwait(false);
        }
    }
}
