using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace UbntSecPilot.Infrastructure.Transactions
{
    public sealed class MongoOutboxRepository : IOutboxRepository
    {
        private readonly IMongoCollection<OutboxRecord> _collection;

        public MongoOutboxRepository(IMongoClient client, IConfiguration config)
        {
            var dbName = config["Mongo:Database"] ?? "secpilot";
            var db = client.GetDatabase(dbName);
            _collection = db.GetCollection<OutboxRecord>("outbox_records");
            _collection.Indexes.CreateOne(new CreateIndexModel<OutboxRecord>(Builders<OutboxRecord>.IndexKeys.Ascending(x => x.TxId).Ascending(x => x.ParticipantKey), new CreateIndexOptions { Unique = true }));
        }

        public async Task SavePreparedAsync(OutboxRecord record)
        {
            if (record is null) throw new ArgumentNullException(nameof(record));
            record.Status = "prepared";
            var filter = Builders<OutboxRecord>.Filter.Eq(x => x.TxId, record.TxId) & Builders<OutboxRecord>.Filter.Eq(x => x.ParticipantKey, record.ParticipantKey);
            var options = new ReplaceOptions { IsUpsert = true };
            await _collection.ReplaceOneAsync(filter, record, options).ConfigureAwait(false);
        }

        public async Task MarkCommittedAsync(string txId, string participantKey)
        {
            var filter = Builders<OutboxRecord>.Filter.Eq(x => x.TxId, txId) & Builders<OutboxRecord>.Filter.Eq(x => x.ParticipantKey, participantKey);
            var update = Builders<OutboxRecord>.Update.Set(x => x.Status, "committed");
            await _collection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = false }).ConfigureAwait(false);
        }

        public async Task MarkAbortedAsync(string txId, string participantKey)
        {
            var filter = Builders<OutboxRecord>.Filter.Eq(x => x.TxId, txId) & Builders<OutboxRecord>.Filter.Eq(x => x.ParticipantKey, participantKey);
            var update = Builders<OutboxRecord>.Update.Set(x => x.Status, "aborted");
            await _collection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = false }).ConfigureAwait(false);
        }

        public async Task<OutboxRecord?> GetAsync(string txId, string participantKey)
        {
            var filter = Builders<OutboxRecord>.Filter.Eq(x => x.TxId, txId) & Builders<OutboxRecord>.Filter.Eq(x => x.ParticipantKey, participantKey);
            return await _collection.Find(filter).FirstOrDefaultAsync().ConfigureAwait(false);
        }
    }
}
