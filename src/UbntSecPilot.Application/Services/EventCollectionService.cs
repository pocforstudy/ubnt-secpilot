using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UbntSecPilot.Domain.Models;
using UbntSecPilot.Domain.Repositories;

namespace UbntSecPilot.Application.Services
{
    /// <summary>
    /// Collects incoming events from external sources and persists them.
    /// </summary>
    public class EventCollectionService
    {
        private readonly INetworkEventRepository _events;

        public EventCollectionService(INetworkEventRepository events)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
        }

        public async Task<NetworkEvent> CollectAsync(string eventId, string source, Dictionary<string, object> payload, DateTime occurredAt)
        {
            if (string.IsNullOrWhiteSpace(eventId)) throw new ArgumentException("eventId required", nameof(eventId));
            if (string.IsNullOrWhiteSpace(source)) throw new ArgumentException("source required", nameof(source));
            payload ??= new Dictionary<string, object>();

            var ev = new NetworkEvent(eventId, source, payload, occurredAt);
            await _events.SaveAsync(ev).ConfigureAwait(false);
            return ev;
        }
    }
}
