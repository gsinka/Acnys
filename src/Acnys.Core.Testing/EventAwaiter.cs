using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Request;
using Acnys.Core.Request.Application;
using Serilog;

namespace Acnys.Core.Testing
{
    public class EventAwaiter<T>
    {
        private readonly ILogger _log;
        private readonly ISendCommand _commandSender;
        private readonly List<AwaiterTask<T>> _tasks = new List<AwaiterTask<T>>();
        private readonly object _lockObj = new object();

        public EventAwaiter(ILogger log, ISendCommand commandSender)
        {
            _log = log.ForContext<EventAwaiter<T>>();
            _commandSender = commandSender;
        }

        public void ProcessEvent(T evnt)
        {
            var eventType = evnt.GetType().Name;

            _log.Debug("Processing event {eventType} with event awaiter service", eventType);

            lock (_lockObj)
            {
                _log.Debug("Checking awaiter tasks ({awaiterCount})", _tasks.Count);
                foreach (var task in _tasks.Where(t => t.Filter(evnt)))
                {
                    _log.Debug("Event {eventType} passed awaiter filter", eventType);
                    task.Event = evnt ;
                    task.ResetEvent.Set();
                }
            }
        }

        public async Task<T> WaitFor(Func<T, bool> filter, TimeSpan timeout)
        {
            using var task = new AwaiterTask<T>(filter, new ManualResetEvent(false));

            lock (_lockObj) _tasks.Add(task);
            task.ResetEvent.WaitOne(timeout);
            lock (_lockObj) _tasks.Remove(task);

            return (T)task.Event;
        }

        public T DoAndWaitForEvent(Action action, Func<T, bool> filter, TimeSpan timeout)
        {
            using var task = new AwaiterTask<T>(filter, new ManualResetEvent(false));

            lock (_lockObj) _tasks.Add(task);
            action();
            task.ResetEvent.WaitOne(timeout);
            lock (_lockObj) _tasks.Remove(task);

            return (T)task.Event;
        }

        public T DoAndWaitForEvent<TCommand>(TCommand command, Func<T, bool> filter, TimeSpan timeout) where TCommand: ICommand
        {
            using var task = new AwaiterTask<T>(filter, new ManualResetEvent(false));

            lock (_lockObj) _tasks.Add(task);

            var commandTask = _commandSender.Send(command);
            task.ResetEvent.WaitOne(timeout);

            lock (_lockObj) _tasks.Remove(task);

            return (T)task.Event;
        }


    }
}