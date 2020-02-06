using Acnys.Core.Abstractions;

namespace Acnys.Core.Infrastructure.Abstractions
{
    public interface IQueryDispatcherSelector
    {
        IDispatchQuery GetDispatcherFor<T>(IQuery<T> query);
    }
}