using System;

namespace Acnys.Core.Request
{
    public abstract class Query<T> : IQuery<T>
    {
        public Guid Id { get; }

        protected Query() : this(Guid.NewGuid())
        {
        }

        protected Query(Guid id)
        {
            Id = id;
        }
    }
}