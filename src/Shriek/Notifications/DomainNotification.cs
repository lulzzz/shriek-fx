using Shriek.Events;
using System;

namespace Shriek.Notifications
{
    public class DomainNotification : IEvent<Guid>
    {
        public Guid DomainNotificationId { get; }
        public string Key { get; }
        public string Value { get; }

        public int Version { get; set; }

        public Guid AggregateId => DomainNotificationId;

        public DateTime Timestamp => DateTime.Now;

        public DomainNotification(string key, string value)
        {
            DomainNotificationId = Guid.NewGuid();
            Version = 1;
            Key = key;
            Value = value;
        }
    }
}