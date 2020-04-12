using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Abstractions;

namespace Acnys.Core.Infrastructure.Abstractions
{
    /// <summary>
    /// Command dispatcher interface
    /// </summary>
    /// <remarks>The command dispatcher dispatch commands from external ports to the application layer</remarks>
    public interface IDispatchCommand
    {
        /// <summary>
        /// Dispatch command
        /// </summary>
        /// <typeparam name="T">Command type</typeparam>
        /// <param name="command">Command</param>
        /// <param name="arguments">Command arguments</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Returns the dispatcher task</returns>
        Task Dispatch<T>(T command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default) where T : ICommand;
    }
}