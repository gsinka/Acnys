using System.Threading;
using System.Threading.Tasks;

namespace Acnys.Core.Request.Application
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
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Returns the dispatcher task</returns>
        Task Dispatch<T>(T command, CancellationToken cancellationToken = default) where T : ICommand;
    }
}