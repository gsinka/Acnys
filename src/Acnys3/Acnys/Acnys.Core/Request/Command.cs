using System;

namespace Acnys.Core.Request
{
    /// <summary>
    /// Command object
    /// </summary>
    public abstract class Command : ICommand, ICausedBy, ICorrelatedBy
    {
        /// <inheritdoc />
        public Guid Id { get; }

        /// <inheritdoc />
        public Guid? CausationId { get; }

        /// <inheritdoc />
        public Guid? CorrelationId { get; }

        /// <inheritdoc />
        /// <summary>
        /// Create new command
        /// </summary>
        /// <param name="causationId">Causation id</param>
        /// <param name="correlationId">Correlation id</param>
        protected Command(Guid? causationId = null, Guid? correlationId = null) 
            : this(Guid.NewGuid(), causationId, correlationId)
        { }

        /// <summary>
        /// Create new command
        /// </summary>
        /// <param name="id">Command id</param>
        /// <param name="causationId">Causation id</param>
        /// <param name="correlationId">Correlation id</param>
        protected Command(Guid id, Guid? causationId = null, Guid? correlationId = null)
        {
            Id = id;
            CausationId = causationId;
            CorrelationId = correlationId;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        protected bool Equals(Command other)
        {
            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((Command)obj);
        }
    }
}