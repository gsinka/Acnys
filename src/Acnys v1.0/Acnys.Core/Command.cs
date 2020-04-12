using System;
using Acnys.Core.Abstractions;

namespace Acnys.Core
{
    /// <summary>
    /// Command object
    /// </summary>
    public abstract class Command : ICommand
    {
        /// <inheritdoc />
        public Guid RequestId { get; }

        /// <summary>
        /// Create new command
        /// </summary>
        /// <param name="requestId">Command id</param>
        protected Command(Guid? requestId = null)
        {
            RequestId = requestId ?? Guid.NewGuid();
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

        public static bool operator ==(Command command1, Command command2)
        {
            if ((object)command1 == null) return (object)command2 == null;
            if ((object)command2 == null) return false;
            return command1.Equals(command2);
        }

        public static bool operator !=(Command command1, Command command2)
        {
            return !(command1 == command2);
        }
    }
}