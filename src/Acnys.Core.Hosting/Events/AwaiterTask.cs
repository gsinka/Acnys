using System;
using System.Threading;
using Acnys.Core.Eventing;

namespace Acnys.Core.Hosting.Events
{
    internal class AwaiterTask
    {
        public Func<IEvent, bool> EventFilter { get; }
        public EventWaitHandle WaitHandle { get; }
        public IEvent Event { get; set; }


        public AwaiterTask(Func<IEvent, bool> eventFilter)
        {
            EventFilter = eventFilter;
            WaitHandle = new ManualResetEvent(false);
        }
    }
}
