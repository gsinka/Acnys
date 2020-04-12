using System;
using System.Threading.Tasks;

namespace Acnys.Core.Domain
{
    public static class AggregateRepositoryExtensions
    {
        public static async Task Execute<T>(this IAggregateRepository repository, Guid aggregateId, Action<T> action) where T : IAggregateRoot
        {
            var aggregate = await repository.GetById<T>(aggregateId);
            action(aggregate);
            await repository.Save(aggregate);
        }

        public static async Task Execute<T>(this IAggregateRepository<T> repository, Guid aggregateId, Action<T> action) where T : IAggregateRoot
        {
            var aggregate = await repository.GetById(aggregateId);
            action(aggregate);
            await repository.Save(aggregate);
        }
    }
}