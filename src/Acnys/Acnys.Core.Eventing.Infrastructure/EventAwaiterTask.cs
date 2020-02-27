using System;
using System.Collections.Generic;
using System.Threading;
using Acnys.Core.Eventing.Abstractions;

namespace Acnys.Core.Eventing.Infrastructure
{
    internal class EventAwaiterTask
    {
        public Func<IEvent, IDictionary<string, object>, bool> EventFilter { get; }
        public Type EventType { get; }
        public EventWaitHandle WaitHandle { get; }
        public IEvent Event { get; set; }

        public EventAwaiterTask(Func<IEvent, IDictionary<string, object>, bool> eventFilter, Type eventType)
        {
            EventFilter = eventFilter;
            EventType = eventType;
            WaitHandle = new ManualResetEvent(false);
        }
    }
}