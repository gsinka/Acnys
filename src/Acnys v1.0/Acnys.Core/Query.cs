using System;
using Acnys.Core.Abstractions;

namespace Acnys.Core
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