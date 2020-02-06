using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Eventing;

namespace Acnys.Core.Request.Application
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
        /// <param name="evnt"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task Dispatch<T>(T evnt, CancellationToken cancellationToken = default) where T : IEvent;
    }
}