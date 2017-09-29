using Microsoft.Extensions.DependencyInjection;
using Shriek.Domains;
using Shriek.Events;
using Shriek.Storage;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Shriek.Commands
{
    public class DefaultCommandContext : ICommandContext, ICommandContextSave
    {
        private readonly ConcurrentQueue<IAggregateRoot> aggregates;
        private static readonly object _lock = new object();
        private readonly IEventBus eventBus;
        private readonly IEventStorage eventStorage;

        public DefaultCommandContext(IServiceProvider Container)
        {
            eventStorage = Container.GetService<IEventStorage>();
            eventBus = Container.GetService<IEventBus>();
            aggregates = new ConcurrentQueue<IAggregateRoot>();
        }

        public IDictionary<string, object> Items => new Dictionary<string, object>();

        /// <summary>
        /// 从内存获取聚合，获取不到则使用委托从数据库获取
        /// </summary>
        /// <typeparam name="TAggregateRoot"></typeparam>
        /// <param name="key"></param>
        /// <param name="initFromRepository"></param>
        /// <returns></returns>
        TAggregateRoot ICommandContext.GetAggregateRoot<TKey, TAggregateRoot>(TKey key, Func<TAggregateRoot> initFromRepository)
        {
            var obj = GetById<TAggregateRoot, TKey>(key);
            if (obj == null)
                obj = initFromRepository();

            if (obj != null)
                aggregates.Enqueue(obj);

            return obj;
        }

        private TAggregateRoot GetById<TAggregateRoot, TKey>(TKey id)
            where TAggregateRoot : IAggregateRoot<TKey>, IEventProvider, new()
            where TKey : IEquatable<TKey>
        {
            return eventStorage.Source<TAggregateRoot, TKey>(id);
        }

        public void Save()
        {
            for (var i = 0; i < aggregates.Count; i++)
            {
                if (aggregates.TryDequeue(out var root) && root.CanCommit)
                {
                    SaveAggregateRoot(root);
                }
            }
        }

        private void SaveAggregateRoot<TKey>(AggregateRoot<TKey> aggregate)
            // where TAggregateRoot : IAggregateRoot, IEventProvider, IAggregateRoot<TKey>
            where TKey : IEquatable<TKey>
        {
            if (!aggregate.GetUncommittedChanges().Any()) return;

            //在锁内程序执行过程中，会有多次对该聚合根的更改请求
            lock (_lock)
            {
                //如果不是新增事件
                if (aggregate.Version != -1)
                {
                    var lastestEvent = eventStorage.GetLastEvent(aggregate.AggregateId);
                    if (lastestEvent != null && lastestEvent.Version != aggregate.Version)
                    {
                        throw new Exception("事件库中该聚合的状态版本与当前传入聚合状态版本不同，可能已被更新");
                    }
                }

                //保存到事件存储
                eventStorage.SaveAggregateRoot<TAggregateRoot, TKey>(aggregate);
                foreach (var @event in aggregate.GetUncommittedChanges())
                {
                    eventBus.Publish(@event);
                }

                aggregate.MarkChangesAsCommitted();
            }
        }

        TAggregateRoot ICommandContext.GetAggregateRoot<TKey, TAggregateRoot>(TKey key)
        {
            var obj = GetById<TAggregateRoot, TKey>(key);
            if (obj != null)
                aggregates.Enqueue(obj);

            return obj;
        }
    }
}