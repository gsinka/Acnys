using System;
using Acnys.Core.Eventing;

namespace WebApplication1
{
    public class TestEvent : Event
    {
        public string Data { get; }

        public TestEvent(string data, Guid? eventId = null) : base(eventId)
        {
            Data = data;
        }
    }
}