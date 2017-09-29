using Shriek.Events;
using Shriek.EventSourcing;
using Shriek.Storage;
using Shriek.Storage.Mementos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Shriek.EventStorage.EFCore
{
    public class EventStorageSQLRepository : IEventStorageRepository, IMementoRepository
    {
        private EventStorageSQLContext context;

        public EventStorageSQLRepository(EventStorageSQLContext context)
        {
            this.context = context;
        }

        public void Dispose()
        {
            context.Dispose();
        }

        public Memento<TKey> GetMemento<TKey>(TKey aggregateId) where TKey : IEquatable<TKey>
        {
            return context.Set<Memento<TKey>>().Where(m => m.AggregateId.Equals(aggregateId))
                .OrderBy(m => m.Version).LastOrDefault();
        }

        public void SaveMemento<TKey>(Memento<TKey> memento) where TKey : IEquatable<TKey>
        {
            context.Set<Memento<TKey>>().Add(memento);
            context.SaveChanges();
        }

        public void Store<TKey>(StoredEvent<TKey> theEvent) where TKey : IEquatable<TKey>
        {
            context.Set<StoredEvent<TKey>>().Add(theEvent);
            context.SaveChanges();
        }

        public IEvent GetLastEvent<TKey>(TKey aggregateId) where TKey : IEquatable<TKey>
        {
            return context.Set<StoredEvent<TKey>>().Where(e => e.AggregateId.Equals(aggregateId))
                .OrderBy(e => e.Timestamp).LastOrDefault();
        }

        public IEnumerable<StoredEvent<TKey>> GetEvents<TKey>(TKey aggregateId, int afterVersion = 0) where TKey : IEquatable<TKey>
        {
            return context.Set<StoredEvent<TKey>>().Where(e => e.AggregateId.Equals(aggregateId) && e.Version >= afterVersion);
        }
    }
}