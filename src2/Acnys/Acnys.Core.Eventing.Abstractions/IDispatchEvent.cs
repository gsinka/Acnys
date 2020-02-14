using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Acnys.Core.Eventing.Abstractions
{
    /// <summary>
    /// Event dispatcher interface
    /// </summary>
    public interface IDispatchEvent
    {
        /// <summary>
        /// Dispatch event to the registred handlers
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="event"></param>
        /// <param name="arguments">Event arguments</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task Dispatch<T>(T @event, IDictionary<string, object> arguments, CancellationToken cancellationToken = default) where T : IEvent;
    }
}