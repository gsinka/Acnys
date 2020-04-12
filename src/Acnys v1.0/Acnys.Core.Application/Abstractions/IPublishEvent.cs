using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Abstractions;

namespace Acnys.Core.Application.Abstractions
{
    /// <summary>
    /// Event publisher interface
    /// </summary>
    public interface IPublishEvent
    {
        /// <summary>
        /// Publish event
        /// </summary>
        /// <typeparam name="T">Event type</typeparam>
        /// <param name="event">Event</param>
        /// <param name="arguments">Event arguments</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        Task Publish<T>(T @event, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default) where T : IEvent;
    }
}