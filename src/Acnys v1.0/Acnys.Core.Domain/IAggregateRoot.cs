using System;
using System.Collections.Generic;
using System.Text;
using Acnys.Core.Abstractions;

namespace Acnys.Core.Domain
{
    public interface IAggregateRoot
    {
        Guid AggregateId { get; }
        long AggregateVersion { get; }

        IEnumerable<IEvent> GetUncommittedEvents();
        void ClearUncommittedEvents();
    }
}
