using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Eventing;
using Acnys.Core.Request.Application;
using Serilog;

namespace Acnys.Core.Testing
{
    public class EventAwaiter : IHandleEvent
    {
        private readonly ILogger _log;
        private readonly List<EventTask> _tasks = new List<EventTask>();
        private readonly object _lockObj = new object();

        public EventAwaiter(ILogger log)
        {
            _log = log.ForContext<EventAwaiter>();
        }

        public Task<T> GetAwaiter<T>(Func<T, bool> filter, TimeSpan timeout, CancellationToken cancellationToken = default) where T : class, IEvent
        {
            var tokenSource = new CancellationTokenSource(timeout);

            return Task.Run(() =>
            {
                _log.Debug("Creating awaiter for event");

                var task = new EventTask(o => filter(o as T), tokenSource);

                lock (_lockObj) _tasks.Add(task);

                WaitHandle.WaitAny(new[] {tokenSource.Token.WaitHandle, cancellationToken.WaitHandle });

                _log.Debug("Finished waiting for event");
                
                if (task.Event == null)
                {
                    _log.Warning("Did not get awaited event in time");
                }

                lock (_lockObj) _tasks.Remove(task);

                return Task.FromResult((T)task.Event);

            }, cancellationToken);
        }

        public Task Handle(IEvent evnt, CancellationToken cancellationToken = default)
        {
            lock (_lockObj)
            {
                _log.Verbose("Evaluating awaiters ({awaiterCount})", _tasks.Count);
                
                foreach (var task in _tasks)
                {
                    if (!task.Filter(evnt)) continue;

                    _log.Debug("Completing awaiter");
                    task.Event = evnt;
                    task.StoppingTokenSource.Cancel();
                }
            }

            return Task.CompletedTask;
        }

        private class EventTask
        {
            public Func<IEvent, bool> Filter { get; }
            public CancellationTokenSource StoppingTokenSource { get; }
            public IEvent Event { get; set; }
            
            public EventTask(Func<IEvent, bool> filter, CancellationTokenSource stoppingTokenSource)
            {
                Filter = filter;
                StoppingTokenSource = stoppingTokenSource;
            }
        }
    }
}
