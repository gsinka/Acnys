using System.Threading;
using System.Threading.Tasks;

namespace Acnys.Core.Request.ReadModel
{
    public interface IHandleQuery<in TQuery, TResult> where TQuery : IQuery<TResult>
    {
        Task<TResult> Handle(TQuery query, CancellationToken cancellationToken = default);
    }
}