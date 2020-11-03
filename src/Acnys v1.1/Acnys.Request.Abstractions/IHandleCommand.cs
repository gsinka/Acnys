using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Acnys.Request.Abstractions
{
    public interface IHandleCommand<in T> where T: ICommand
    {
        Task HandleAsync(T command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default);
    }
}