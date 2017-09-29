using Shriek.Events;
using Shriek.EventSourcing;
using Shriek.Storage;
using Shriek.Storage.Mementos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Shriek.EventStorage.LiteDB
{
    public class EventStorageRepository : IEventStorageRepository, IMementoRepository
    {
        private readonly EventStorageLiteDatabase _liteDatabase;

        public EventStorageRepository(EventStorageLiteDatabase liteDatabase)
        {
            this._liteDatabase = liteDatabase;
        }

        public IEnumerable<StoredEvent<TKey>> GetEvents<TKey>(TKey aggregateId, int afterVersion = 0)
            where TKey : IEquatable<TKey>
        {
            return this._liteDatabase.GetCollection<StoredEvent<TKey>>().Find(e => e.AggregateId.Equals(aggregateId) && e.Version >= afterVersion);
        }

        public void Store<TKey>(StoredEvent<TKey> theEvent)
            where TKey : IEquatable<TKey>
        {
            this._liteDatabase.GetCollection<StoredEvent<TKey>>().Insert(theEvent);
        }

        public void Dispose()
        {
            this._liteDatabase.Dispose();
        }

        public IEvent GetLastEvent<TKey>(TKey aggregateId)
        where TKey : IEquatable<TKey>
        {
            return this._liteDatabase.GetCollection<StoredEvent<TKey>>()
                .Find(e => e.AggregateId.Equals(aggregateId)).OrderBy(e => e.Timestamp).LastOrDefault();
        }

        public Memento<TKey> GetMemento<TKey>(TKey aggregateId)
            where TKey : IEquatable<TKey>
        {
            return this._liteDatabase.GetCollection<Memento<TKey>>()
                .Find(m => m.AggregateId.Equals(aggregateId)).OrderBy(m => m.Version).LastOrDefault();
        }

        public void SaveMemento<TKey>(Memento<TKey> memento)
            where TKey : IEquatable<TKey>
        {
            this._liteDatabase.GetCollection<Memento<TKey>>().Insert(memento);
        }
    }
}