using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Eventing;
using Acnys.Core.Request;
using Acnys.Core.Request.Application;
using Serilog;

namespace Acnys.Core.Hosting.Events
{
    public class RequestService : IHandleEvent, IRequestService
    {
        private readonly ILogger _log;
        private readonly ISendCommand _commandSender;
        private readonly ISendQuery _querySender;
        private readonly object _lockObject = new object();
        private readonly List<AwaiterTask> _tasks = new List<AwaiterTask>();

        public RequestService(ILogger log, ISendCommand commandSender, ISendQuery querySender)
        {
            _log = log;
            _commandSender = commandSender;
            _querySender = querySender;
        }
        
        public Task Handle(IEvent evnt, CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                _log.Debug("Processing event awaiters");

                _log.Verbose("Checking {awaiterCount} awaiters", _tasks.Count);

                lock (_lockObject)
                {
                    foreach (var task in _tasks.Where(t => t.EventFilter(evnt)))
                    {
                        _log.Verbose("Event awaiter task {awaiterTaskId} matching event", task.GetHashCode());

                        task.Event = evnt;
                        task.WaitHandle.Set();
                    }
                }

            }, cancellationToken);
        }

        public async Task<T> ExeSendCommandAndWaitForEvent<T, TCommand>(TCommand command, Func<T, bool> eventFilter, TimeSpan timeout, CancellationToken cancellationToken) 
            where T : IEvent where TCommand : ICommand
        {
            var stoppingTokenSource = new CancellationTokenSource(timeout);
            stoppingTokenSource.Token.ThrowIfCancellationRequested();
            
            cancellationToken.Register(() =>
            {
                stoppingTokenSource?.Cancel();
            });

            var eventAwaiterTask = GetEventAwaiter(eventFilter, stoppingTokenSource.Token);
            
            await _commandSender.Send(command, stoppingTokenSource.Token);

            var result = await eventAwaiterTask;
            stoppingTokenSource.Dispose();
            
            return result;
        }

        public Task<T> GetEventAwaiter<T>(Func<T, bool> eventFilter, CancellationToken cancellationToken = default) where T : IEvent
        {
            var task = new AwaiterTask(@event => eventFilter((T)@event));

            _log.Debug("Creating event awaiter task {awaiterTaskId}", task.GetHashCode());

            lock (_lockObject)
            {
                _tasks.Add(task);
            }

            return new TaskFactory<T>().StartNew(() =>
            {
                cancellationToken.Register(() =>
                {
                    _log.Debug("Awaiting event on task {awaiterTaskId} cancelled", task.GetHashCode());
                    task.WaitHandle.Set();
                });
                
                task.WaitHandle.WaitOne();

                _log.Verbose("Removing awaiting event task {awaiterTaskId}", task.GetHashCode());

                lock (_lockObject)
                {
                    _tasks.Remove(task);
                }

                return (T)task.Event;

            }, cancellationToken);
        }
    }
}