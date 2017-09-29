﻿using Shriek.Domains;
using Shriek.Events;
using Shriek.Storage.Mementos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Shriek.Storage
{
    public class InMemoryEventStorage : IEventStorage, IEventOriginator
    {
        private List<IEvent> _events;
        private List<IMemento> _mementoes;

        public InMemoryEventStorage()
        {
            _events = new List<IEvent>();
            _mementoes = new List<IMemento>();
        }

        public IEnumerable<IEvent<TKey>> GetEvents<TKey>(TKey aggregateId, int afterVersion = 0) where TKey : IEquatable<TKey>
        {
            var list = _events.Select(x => x as IEvent<TKey>);

            var events = list.Where(e => e.AggregateId.Equals(aggregateId) && e.Version >= afterVersion);

            return events;
        }

        public IEvent<TKey> GetLastEvent<TKey>(TKey aggregateId) where TKey : IEquatable<TKey>
        {
            var list = _events.Select(x => x as IEvent<TKey>);

            return list.Where(e => e.AggregateId.Equals(aggregateId))
                .OrderBy(e => e.Version).LastOrDefault();
        }

        public void SaveAggregateRoot<TAggregateRoot, TKey>(TAggregateRoot aggregate)
            where TAggregateRoot : IAggregateRoot<TKey>, IEventProvider, IOriginator<TKey>
            where TKey : IEquatable<TKey>
        {
            var uncommittedChanges = aggregate.GetUncommittedChanges();
            var version = aggregate.Version;

            foreach (var @event in uncommittedChanges)
            {
                version++;
                if (version > 2)
                {
                    if (version % 3 == 0)
                    {
                        var originator = (IOriginator<TKey>)aggregate;
                        var memento = originator.GetMemento();
                        memento.Version = version;
                        SaveMemento(memento);
                    }
                }
                @event.Version = version;
                Save(@event);
            }
        }

        public void SaveMemento<TKey>(Memento<TKey> memento)
            where TKey : IEquatable<TKey>
        {
            _mementoes.Add(memento);
        }

        public void Save<TEvent>(TEvent @event) where TEvent : IEvent
        {
            _events.Add(@event);
        }

        public TAggregateRoot Source<TAggregateRoot, TKey>(TKey aggregateId)
            where TAggregateRoot : IAggregateRoot, IEventProvider, new()
            where TKey : IEquatable<TKey>
        {
            //获取该记录的所有缓存事件
            IEnumerable<IEvent> events;
            Memento<TKey> memento = null;
            var obj = new TAggregateRoot();

            if (obj is IOriginator<TKey>)
            {
                //获取该记录的更改快照
                memento = GetMemento(aggregateId);
            }

            if (memento != null)
            {
                //获取该记录最后一次快照之后的更改，避免加载过多历史更改
                events = GetEvents(aggregateId, memento.Version);
                //从快照恢复
                ((IOriginator<TKey>)obj).SetMemento(memento);
            }
            else
            {
                //获取所有历史更改记录
                events = GetEvents(aggregateId);
            }

            if (memento == null && !events.Any())
                return default(TAggregateRoot);

            //重现历史更改
            obj.LoadsFromHistory(events);
            return obj;
        }

        public Memento<TKey> GetMemento<TKey>(TKey aggregateId)
            where TKey : IEquatable<TKey>
        {
            var list = _mementoes.Select(x => x as Memento<TKey>);

            return list.LastOrDefault();
        }
    }
}