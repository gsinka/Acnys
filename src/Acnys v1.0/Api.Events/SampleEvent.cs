using System;
using Acnys.Core;

namespace Api.Events
{
    public class SampleEvent : Event
    {
        public SampleEvent(Guid? eventId = null) : base(eventId ?? Guid.NewGuid())
        {
        }
    }
}