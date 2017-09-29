using System;

namespace Shriek.Domains
{
    public interface IAggregateRoot
    {
        int Version { get; }

        bool CanCommit { get; }
    }

    public interface IAggregateRoot<out TKey> : IAggregateRoot
        where TKey : IEquatable<TKey>
    {
        TKey AggregateId { get; }
    }
}