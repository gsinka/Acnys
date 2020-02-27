using System;
using Acnys.Core.Request.Abstractions;

namespace Acnys.Core.Request
{
    public abstract class Query<T> : IQuery<T>
    {
        public Guid RequestId { get; }

        protected Query() : this(Guid.NewGuid())
        {
        }

        protected Query(Guid requestId)
        {
            RequestId = requestId;
        }
    }
}