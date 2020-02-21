using System;

namespace Acnys.Core.Eventing.Tests
{
    public class TestEvent : Event
    {
        public TestEvent(Guid? eventId = null) : base(eventId)
        {
        }
    }
}