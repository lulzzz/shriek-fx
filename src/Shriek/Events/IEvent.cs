using System;
using Shriek.Messages;

namespace Shriek.Events
{
    public interface IEvent<TKey> : IEvent where TKey : IEquatable<TKey>
    {
        TKey AggregateId { get; }
    }

    public interface IEvent : IMessage

    {
        int Version { get; set; }

        DateTime Timestamp { get; }
    }
}