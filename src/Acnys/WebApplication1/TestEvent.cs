using System;
using Acnys.Core.Eventing;

namespace WebApplication1
{
    //[HumanReadableInformation("Test Event", "Test event's deteailed description")]
    public class TestEvent : Event
    {
        public string Data { get; }

        public TestEvent(string data, Guid? eventId = null) : base(eventId)
        {
            Data = data;
        }
    }
}