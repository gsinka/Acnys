using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Request.Abstractions;

namespace Acnys.Core.Request
{
    /// <summary>
    /// Query dispatcher interface
    /// </summary>
    public interface IDispatchQuery
    {
        /// <summary>
        /// Dispatch query to the registered query handler
        /// </summary>
        /// <typeparam name="TResult">Type of query result</typeparam>
        /// <param name="query">Query to handle</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        Task<TResult> Dispatch<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);
    }
}