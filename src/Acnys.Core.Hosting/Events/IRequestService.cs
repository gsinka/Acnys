using System;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Eventing;
using Acnys.Core.Request;

namespace Acnys.Core.Hosting.Events
{
    public interface IRequestService
    {
        Task<T> GetEventAwaiter<T>(Func<T, bool> eventFilter, CancellationToken cancellationToken = default) 
            where T : IEvent;

        Task<T> ExeSendCommandAndWaitForEvent<T, TCommand>(TCommand command, Func<T, bool> eventFilter, TimeSpan timeout, CancellationToken cancellationToken) 
            where T : IEvent where TCommand : ICommand;
    }
}