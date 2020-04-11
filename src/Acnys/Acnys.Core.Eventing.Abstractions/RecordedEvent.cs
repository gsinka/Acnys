using System;
using System.Collections.Generic;
using Acnys.Core.Eventing.Abstractions;

namespace Acnys.Core.Eventing.Infrastructure
{
    public class RecordedEvent
    {
        public DateTime TimeStamp { get; }
        public IEvent Event { get; }
        public IDictionary<string, object> Arguments { get; }

        public RecordedEvent(DateTime timeStamp, IEvent @event, IDictionary<string, object> arguments)
        {
            TimeStamp = timeStamp;
            Event = @event;
            Arguments = arguments;
        }
    }
}