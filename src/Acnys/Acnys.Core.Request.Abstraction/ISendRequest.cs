using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Acnys.Core.Request.Abstractions
{
    public interface ISendRequest
    {
        Task Send<T>(T command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default) where T : ICommand;

        Task<T> Send<T>(IQuery<T> query, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default);
    }
}