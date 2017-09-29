﻿using Shriek.Domains;
using Shriek.Events;
using System;
using System.Collections.Generic;
using Shriek.Storage.Mementos;

namespace Shriek.Storage
{
    public interface IEventStorage
    {
        IEnumerable<IEvent<TKey>> GetEvents<TKey>(TKey aggregateId, int afterVersion = 0) where TKey : IEquatable<TKey>;

        void SaveAggregateRoot<TAggregateRoot, TKey>(TAggregateRoot aggregate)
            where TAggregateRoot : IAggregateRoot<TKey>, IEventProvider
            where TKey : IEquatable<TKey>;

        /// <summary>
        /// 事件溯源，获取聚合根
        /// </summary>
        /// <typeparam name="TAggregateRoot">聚合根类型</typeparam>
        ///    /// <typeparam name="TKey">聚合根Id类型</typeparam>
        /// <param name="aggregateId">唯一标识</param>
        TAggregateRoot Source<TAggregateRoot, TKey>(TKey aggregateId)
            where TAggregateRoot : IAggregateRoot, IEventProvider, new()
            where TKey : IEquatable<TKey>;

        IEvent<TKey> GetLastEvent<TKey>(TKey aggregateId) where TKey : IEquatable<TKey>;

        void Save<TEvent>(TEvent @event) where TEvent : IEvent;
    }
}