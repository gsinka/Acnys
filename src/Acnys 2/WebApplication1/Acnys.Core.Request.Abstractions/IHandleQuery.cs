using System.Threading;
using System.Threading.Tasks;

namespace Acnys.Core.Request.Abstractions
{
    public interface IHandleQuery<in TQuery, TResult> where TQuery : IQuery<TResult>
    {
        Task<TResult> Handle(TQuery query, CancellationToken cancellationToken = default);
    }
}