using System;
using System.Threading.Tasks;

namespace Acnys.Core.Domain
{
    public interface IAggregateRepository
    {
        Task<T> GetById<T>(Guid aggregateId) where T : IAggregateRoot;
        Task Save<T>(T aggregateRoot);
    }

    public interface IAggregateRepository<T> where T : IAggregateRoot
    {
        Task<T> GetById(Guid aggregateId);
        Task Save(T aggregateRoot);
    }
}