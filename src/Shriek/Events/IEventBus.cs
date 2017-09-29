using System;

namespace Shriek.Events
{
    public interface IEventBus
    {
        void Publish<TEvent, TKey>(TEvent @event)
            where TEvent : IEvent<TKey>
            where TKey : IEquatable<TKey>;
    }
}