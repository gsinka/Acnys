using System.Collections.Generic;
using Acnys.Core.Eventing;

namespace Acnys.Core.Aggregates
{
    public interface IEventSourcedAggregateRoot : IAggregateRoot
    {
        IList<IEvent> GetUncommittedEvents();
        void ClearUncommittedEvents();
    }
}