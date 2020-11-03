using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Acnys.Request.Abstractions
{
    public interface IHandleQuery<in TQuery, TResult> where TQuery : IQuery<TResult>
    {
        Task<TResult> HandleAsync(TQuery query, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default);
    }
}