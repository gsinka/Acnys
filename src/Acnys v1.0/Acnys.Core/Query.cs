using System;
using Acnys.Core.Abstractions;

namespace Acnys.Core
{
    public abstract class Query<T> : IQuery<T>
    {
        public Guid RequestId { get; }

        protected Query(Guid requestId)
        {
            RequestId = requestId;
        }

        public override int GetHashCode()
        {
            return RequestId.GetHashCode();
        }

        protected bool Equals(Query<T> other)
        {
            return RequestId.Equals(other.RequestId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((Query<T>)obj);
        }
    }
}