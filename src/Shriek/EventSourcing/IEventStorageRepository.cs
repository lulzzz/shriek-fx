using Shriek.Events;
using Shriek.Storage;
using System;
using System.Collections.Generic;

namespace Shriek.EventSourcing
{
    public interface IEventStorageRepository : IDisposable
    {
        void Store<TKey>(StoredEvent<TKey> theEvent)
            where TKey : IEquatable<TKey>;

        IEvent GetLastEvent<TKey>(TKey aggregateId)
            where TKey : IEquatable<TKey>;

        IEnumerable<StoredEvent<TKey>> GetEvents<TKey>(TKey aggregateId, int afterVersion = 0)
            where TKey : IEquatable<TKey>;
    }
}