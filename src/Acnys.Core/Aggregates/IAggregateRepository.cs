using System;
using System.Threading.Tasks;

namespace Acnys.Core.Aggregates
{
    public interface IAggregateRepository
    {
        Task<T> GetById<T>(Guid aggregateId) where T : IAggregateRoot;
        Task Save(IAggregateRoot aggregate);
    }
}