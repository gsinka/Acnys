using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Eventing.Abstractions;
using Serilog;

namespace Acnys.Core.Eventing.Infrastructure
{
    public class EventAwaiterService
    {
        private readonly ILogger _log;
        private readonly object _lockObject = new object();
        private readonly IList<EventAwaiterTask> _tasks = new List<EventAwaiterTask>();

        public EventAwaiterService(ILogger log)
        {
            _log = log;
        }

        public Task ProcessEvent<T>(T @event, IDictionary<string, object> arguments, CancellationToken cancellationToken) where T : IEvent
        {
            return Task.Run(() =>
            {
                _log.Debug("Processing event awaiters");
                _log.Verbose("Checking {awaiterCount} awaiters", _tasks.Count);

                lock (_lockObject)
                {
                    foreach (var task in _tasks.Where(x => x.EventType == typeof(T) && x.EventFilter(@event, arguments)))
                    {
                        _log.Verbose("Event awaiter task {awaiterTaskId} matching event", task.GetHashCode());

                        task.Event = @event;
                        task.WaitHandle.Set();
                    }
                }

            }, cancellationToken);
        }

        public Task<T> GetEventAwaiter<T>(Func<T, IDictionary<string, object>, bool> eventFilter, CancellationToken cancellationToken = default) 
            where T : IEvent
        {
            var task = new EventAwaiterTask((evnt, args) => eventFilter((T)evnt, args), typeof(T));
            
            cancellationToken.Register(() =>
            {
                _log.Debug("Awaiting event on task {awaiterTaskId} cancelled", task.GetHashCode());
                task.WaitHandle.Set();
            });

            _log.Debug("Creating event awaiter task {awaiterTaskId}", task.GetHashCode());

            lock (_lockObject) { _tasks.Add(task); }

            return new TaskFactory<T>().StartNew(() =>
            {
                task.WaitHandle.WaitOne();

                _log.Verbose("Removing awaiting event task {awaiterTaskId}", task.GetHashCode());

                lock (_lockObject) { _tasks.Remove(task); }

                return (T)task.Event;

            }, cancellationToken);
        }
    }
}
