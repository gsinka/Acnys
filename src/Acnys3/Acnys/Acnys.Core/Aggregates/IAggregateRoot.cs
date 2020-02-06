using System;

namespace Acnys.Core.Aggregates
{
    public interface IAggregateRoot
    {
        Guid AggregateId { get; }
        long AggregateVersion { get; }
    }
}
