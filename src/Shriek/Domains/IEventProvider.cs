using System;
using Shriek.Events;
using System.Collections.Generic;

namespace Shriek.Domains
{
    public interface IEventProvider
    {
        void LoadsFromHistory(IEnumerable<IEvent> history);

        IEnumerable<IEvent> GetUncommittedChanges();

        void MarkChangesAsCommitted();
    }
}