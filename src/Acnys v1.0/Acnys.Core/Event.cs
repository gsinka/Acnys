using System;
using Acnys.Core.Abstractions;

namespace Acnys.Core
{
    /// <inheritdoc cref="IEvent" />
    /// <summary>
    /// Event object
    /// </summary>
    public abstract class Event : IEvent
    {
        /// <summary>
        /// Event Id
        /// </summary>
        public Guid EventId { get; }

        /// <summary>
        /// Create new event
        /// </summary>
        /// <param name="eventId">Event id</param>
        protected Event(Guid eventId)
        {
            EventId = eventId;
        }

        protected bool Equals(Event other)
        {
            return EventId.Equals(other.EventId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((Event) obj);
        }

        public override int GetHashCode()
        {
            return EventId.GetHashCode();
        }

        public static bool operator ==(Event event1, Event event2)
        {
            if ((object)event1 == null) return (object)event2 == null;
            if ((object)event2 == null) return false;
            return event1.Equals(event2);
        }

        public static bool operator !=(Event event1, Event event2)
        {
            return !(event1 == event2);
        }
    }
}