using Shriek.Messages;
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Shriek.Events
{
    public class InMemoryEventBus : IEventBus, IDisposable
    {
        private readonly IMessagePublisher messageProcessor;
        private readonly ConcurrentDictionary<string, ConcurrentQueue<IEvent>> eventQueueDict = new ConcurrentDictionary<string, ConcurrentQueue<IEvent>>();
        private readonly ConcurrentDictionary<string, Task> taskDict = new ConcurrentDictionary<string, Task>();

        private static Task _task;

        public InMemoryEventBus(IMessagePublisher messageProcessor)
        {
            this.messageProcessor = messageProcessor;
        }

        public void Dispose()
        {
            messageProcessor.Dispose();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Publish<TEvent, TKey>(TEvent @event)
            where TEvent : IEvent<TKey>
            where TKey : IEquatable<TKey>
        {
            if (@event == null) return;

            var eventQueue = eventQueueDict.GetOrAdd(@event.AggregateId.ToString(), new ConcurrentQueue<IEvent>());
            eventQueue.Enqueue(@event);

            if (!taskDict.TryGetValue(@event.AggregateId.ToString(), out var task) || task.IsCompleted || task.IsCanceled || task.IsFaulted)
            {
                task?.Dispose();

                taskDict[@event.AggregateId.ToString()] = Task.Run(() =>
                {
                    while (!eventQueue.IsEmpty && eventQueue.TryDequeue(out var evt))
                        messageProcessor.Send(evt);
                });
            }
        }
    }
}