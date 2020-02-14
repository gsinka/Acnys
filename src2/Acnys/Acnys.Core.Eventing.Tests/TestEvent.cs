using System;

namespace Acnys.Core.Eventing.Tests
{
    public class TestEvent : Event
    {
        public TestEvent(Guid? eventId = null, Guid? causationId = null, Guid? correlationId = null) : base(eventId, causationId, correlationId)
        {
        }
    }
}