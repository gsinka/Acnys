using System;
using Acnys.Core.Abstractions;
using Acnys.Core.Eventing.Abstractions;

namespace Acnys.Core.Eventing
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
        protected Event(Guid? eventId = null)
        {
            EventId = eventId ?? Guid.NewGuid();
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
    }
}