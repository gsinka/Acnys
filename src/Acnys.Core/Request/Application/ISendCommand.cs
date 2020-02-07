using System.Threading;
using System.Threading.Tasks;

namespace Acnys.Core.Request.Application
{
    /// <summary>
    /// Command sender interface
    /// </summary>
    /// <remarks>The command sender sends command to </remarks>
    public interface ISendCommand
    {
        Task Send<T>(T command,  CancellationToken cancellationToken = default) where T : ICommand;
    }
}