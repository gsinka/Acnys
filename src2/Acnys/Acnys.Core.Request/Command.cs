using System;
using Acnys.Core.Abstractions;
using Acnys.Core.Request.Abstractions;

namespace Acnys.Core.Request
{
    /// <summary>
    /// Command object
    /// </summary>
    public abstract class Command : ICommand, ICausedBy, ICorrelatedBy
    {
        /// <inheritdoc />
        public Guid RequestId { get; }

        /// <inheritdoc />
        public Guid? CausationId { get; }

        /// <inheritdoc />
        public Guid? CorrelationId { get; }

        /// <summary>
        /// Create new command
        /// </summary>
        /// <param name="requestId">Command id</param>
        /// <param name="causationId">Causation id</param>
        /// <param name="correlationId">Correlation id</param>
        protected Command(Guid requestId, Guid? causationId = null, Guid? correlationId = null)
        {
            RequestId = requestId;
            CausationId = causationId;
            CorrelationId = correlationId;
        }

        public override int GetHashCode()
        {
            return RequestId.GetHashCode();
        }

        protected bool Equals(Command other)
        {
            return RequestId.Equals(other.RequestId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((Command)obj);
        }
    }
}