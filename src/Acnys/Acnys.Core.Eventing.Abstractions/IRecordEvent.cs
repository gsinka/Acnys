using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Eventing.Infrastructure;

namespace Acnys.Core.Eventing.Abstractions
{
    public interface IRecordEvent : IHandleEvent
    {
        IEnumerable<RecordedEvent> RecordedEvents { get; }
        Task ClearRecordedEvents(CancellationToken cancellationToken = default);

        //Task<T> WaitFor<T>(CancellationToken cancellationToken = default) where T : IEvent;
        //Task<T> WaitFor<T>(Func<T, bool> filter, CancellationToken cancellationToken = default) where T : IEvent;

        Task<T> WaitFor<T>(Func<T, IDictionary<string, object>, bool> filter, TimeSpan timeOut) where T : IEvent;
        Task<T> WaitFor<T>(Func<T, IDictionary<string, object>, bool> filter, CancellationToken cancellationToken = default) where T : IEvent;
    }
}