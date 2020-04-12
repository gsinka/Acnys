using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Abstractions;

namespace Acnys.Core.Application.Abstractions
{
    public interface IHandleQuery<in TQuery, TResult> where TQuery : IQuery<TResult>
    {
        Task<TResult> Handle(TQuery query, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default);
    }
}