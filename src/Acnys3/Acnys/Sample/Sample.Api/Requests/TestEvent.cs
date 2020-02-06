using System;
using Acnys.Core.Eventing;

namespace Sample.Api.Requests
{
    public class TestEvent : Event
    {
        public string Data { get; }

        public TestEvent(string data, Guid? eventId = null, Guid? causationId = null, Guid? correlationId = null) 
            : base(eventId, causationId, correlationId)
        {
            Data = data;
        }
    }
}