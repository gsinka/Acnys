using System;
using Acnys.Core.Abstractions;
using Acnys.Core.Eventing.Abstractions;

namespace Acnys.Core.Eventing
{
    /// <inheritdoc cref="IEvent" />
    /// <summary>
    /// Event object
    /// </summary>
    public abstract class Event : IEvent, ICausedBy, ICorrelatedBy
    {
        /// <summary>
        /// Event Id
        /// </summary>
        public Guid EventId { get; }

        /// <summary>
        /// Causation id
        /// </summary>
        public Guid? CausationId { get; }

        /// <summary>
        /// Correlation id
        /// </summary>
        public Guid? CorrelationId { get; }

        /// <summary>
        /// Create new event
        /// </summary>
        /// <param name="eventId">Event id</param>
        /// <param name="causationId">Causation id</param>
        /// <param name="correlationId">Correlation id</param>
        protected Event(Guid? eventId = null, Guid? causationId = null, Guid? correlationId = null)
        {
            EventId = eventId ?? Guid.NewGuid();
            CausationId = causationId;
            CorrelationId = correlationId;
        }

        protected bool Equals(Event other)
        {
            return EventId.Equals(other.EventId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Event) obj);
        }

        public override int GetHashCode()
        {
            return EventId.GetHashCode();
        }
    }
}