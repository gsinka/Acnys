using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Abstractions;

namespace Acnys.Core.Application.Abstractions
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
        /// <param name="event">Event</param>
        /// <param name="arguments">Event arguments</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        Task Handle(T @event, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default);
    }
    
    public interface IHandleEvent
    {
        Task Handle(IEvent @event, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default);
    }
}