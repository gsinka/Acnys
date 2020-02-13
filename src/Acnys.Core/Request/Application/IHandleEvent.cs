using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Eventing;

namespace Acnys.Core.Request.Application
{
    /// <summary>
    /// Event handler interface
    /// </summary>
    /// <typeparam name="T">Type of event</typeparam>
    public interface IHandleEvent<in T> where T : IEvent
    {
        /// <summary>
        /// Event handler method
        /// </summary>
        /// <param name="evnt">Event</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        Task Handle(T evnt, CancellationToken cancellationToken = default);
    }

    public interface IHandleEvent
    {
        Task Handle(IEvent evnt, CancellationToken cancellationToken = default);
    }
}