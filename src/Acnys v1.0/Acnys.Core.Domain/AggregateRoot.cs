using System;
using System.Collections.Generic;
using Acnys.Core.Abstractions;

namespace Acnys.Core.Domain
{
    public abstract class AggregateRoot : IAggregateRoot
    {
        protected readonly List<IEvent> UncommittedEvents = new List<IEvent>();

        public Guid AggregateId { get; }
        public long AggregateVersion { get; }

        protected AggregateRoot(Guid? aggregateId = default)
        {
            AggregateId = aggregateId ?? Guid.NewGuid();
            AggregateVersion = 0;
        }

        public IEnumerable<IEvent> GetUncommittedEvents()
        {
            return UncommittedEvents;
        }

        public void ClearUncommittedEvents()
        {
            UncommittedEvents.Clear();
        }

        protected virtual void RaiseEvent(IEvent @event)
        {
            UncommittedEvents.Add(@event);
        }
    }
}