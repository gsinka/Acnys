using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Acnys.Core.Request.Abstractions
{
    /// <summary>
    /// Query sender interface
    /// </summary>
    /// <remarks>The query sender sends query to</remarks>
    public interface ISendQuery
    {
        Task<T> Send<T>(IQuery<T> query, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default);
    }
}