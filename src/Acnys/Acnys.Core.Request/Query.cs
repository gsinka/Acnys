using System;
using Acnys.Core.Request.Abstractions;

namespace Acnys.Core.Request
{
    public abstract class Query<T> : IQuery<T>
    {
        public Guid RequestId { get; }

        protected Query(Guid? requestId = default)
        {
            RequestId = requestId ?? Guid.NewGuid();
        }
    }
}