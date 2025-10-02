using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UbntSecPilot.Application.Services;
using UbntSecPilot.Domain.Models;
using UbntSecPilot.Domain.Repositories;
using Xunit;

namespace UbntSecPilot.Application.Tests
{
    public class EventCollectionServiceTests
    {
        private sealed class InMemoryEventsRepo : INetworkEventRepository
        {
            public readonly Dictionary<string, NetworkEvent> Store = new();

            public Task<NetworkEvent> GetByIdAsync(string eventId)
                => Task.FromResult(Store.TryGetValue(eventId, out var ev) ? ev : null!);

            public Task<IEnumerable<NetworkEvent>> GetAllAsync(int limit = 100)
                => Task.FromResult<IEnumerable<NetworkEvent>>(Store.Values);

            public Task<IEnumerable<NetworkEvent>> GetBySourceAsync(string source, int limit = 100)
                => Task.FromResult<IEnumerable<NetworkEvent>>(Array.Empty<NetworkEvent>());

            public Task<IEnumerable<NetworkEvent>> GetByTimeRangeAsync(DateTime startTime, DateTime endTime)
                => Task.FromResult<IEnumerable<NetworkEvent>>(Array.Empty<NetworkEvent>());

            public Task<IEnumerable<NetworkEvent>> GetUnprocessedEventsAsync(int limit = 10)
                => Task.FromResult<IEnumerable<NetworkEvent>>(Array.Empty<NetworkEvent>());

            public Task SaveAsync(NetworkEvent networkEvent)
            {
                Store[networkEvent.EventId] = networkEvent;
                return Task.CompletedTask;
            }

            public Task SaveManyAsync(IEnumerable<NetworkEvent> networkEvents)
            {
                foreach (var ev in networkEvents)
                {
                    Store[ev.EventId] = ev;
                }
                return Task.CompletedTask;
            }

            public Task UpdateAsync(NetworkEvent networkEvent)
            {
                Store[networkEvent.EventId] = networkEvent;
                return Task.CompletedTask;
            }

            public Task DeleteAsync(string eventId)
            {
                Store.Remove(eventId);
                return Task.CompletedTask;
            }
        }

        [Fact]
        public async Task CollectAsync_Persists_Event()
        {
            var repo = new InMemoryEventsRepo();
            var svc = new EventCollectionService(repo);

            var ev = await svc.CollectAsync("evt-1", "unit-test", new Dictionary<string, object>{{"k","v"}}, DateTime.UtcNow);

            Assert.NotNull(ev);
            Assert.True(repo.Store.ContainsKey("evt-1"));
            Assert.Equal("unit-test", repo.Store["evt-1"].Source);
        }
    }
}
