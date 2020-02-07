using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Eventing;

namespace Acnys.Core.Request.Application
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
        /// <param name="evnt">Event</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        Task Publish<T>(T evnt, CancellationToken cancellationToken = default) where T : IEvent;
        
        Task Publish<T>(IList<T> events, CancellationToken cancellationToken = default) where T : IEvent;
    }
}