using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Acnys.Request.Abstractions
{
    /// <summary>
    /// Command sender interface
    /// </summary>
    /// <remarks>The command sender sends command to </remarks>
    public interface ISendCommand
    {
        Task SendAsync<T>(T command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default) where T : ICommand;
    }
}