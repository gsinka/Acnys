﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Abstractions;
using Acnys.Core.Eventing.Abstractions;
using Serilog;

namespace Acnys.Core.Eventing.Infrastructure
{
    public class EventRecorder : IRecordEvent
    {
        private readonly ILogger _log;
        private readonly IClock _clock;
        private readonly int _eventTtl;
        private readonly List<RecordedEvent> _events = new List<RecordedEvent>();
        private readonly IList<WaitTask> _tasks = new List<WaitTask>();
        private readonly object _lockObj = new object();

        public IEnumerable<RecordedEvent> RecordedEvents
        {
            get { lock (_lockObj) { return _events.ToArray();} }
        }

        // ReSharper disable once InconsistentNaming

        public EventRecorder(ILogger log, IClock clock, int eventTTL = 60000)
        {
            _log = log;
            _clock = clock;
            _eventTtl = eventTTL;
        }

        private void RemoveExpiredEvents()
        {
            lock (_lockObj)
            {
                var maxTime = _clock.UtcNow.AddMilliseconds(-_eventTtl);
                foreach (var oldEvent in _events.Where(recordedEvent => recordedEvent.TimeStamp < maxTime).ToList())
                {
                    _log.Debug("Removing expired event {eventType} with age of {eventAge} from event store", oldEvent.Event.GetType().FullName, (_clock.UtcNow - oldEvent.TimeStamp).TotalMilliseconds);
                    _events.Remove(oldEvent);
                }
            }
        }

        public Task Handle(IEvent @event, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            RemoveExpiredEvents();

            _log.Debug("Recording event {eventType} in event store", @event.GetType().FullName);

            lock (_lockObj)
            {
                _events.Add(new RecordedEvent(_clock.UtcNow, @event, arguments));
            }

            return Task.Run(() =>
            {
                _log.Debug("Processing event awaiters for event {eventType}", @event.GetType().FullName);
                _log.Verbose("Checking {awaiterCount} awaiter(s)", _tasks.Count);

                lock (_lockObj)
                {
                    foreach (var task in _tasks.Where(x => @event.GetType() == x.EventType && x.EventFilter(@event, arguments)))
                    {
                        _log.Verbose("Event awaiter task {awaiterTaskId} matching event {eventType}", task.GetHashCode(), @event.GetType().FullName);

                        task.Event = @event;
                        task.WaitHandle.Set();
                    }
                }

            }, cancellationToken);
        }

        public Task ClearRecordedEvents(CancellationToken cancellationToken = default)
        {
            lock (_lockObj)
            {
                _events.Clear();
            }
            
            return Task.CompletedTask;
        }

        public async Task<T> WaitFor<T>(Func<T, IDictionary<string, object>, bool> filter, TimeSpan timeOut) where T : IEvent
        {
            return await WaitFor<T>(filter, new CancellationTokenSource(timeOut).Token);
        }

        public Task<T> WaitFor<T>(Func<T, IDictionary<string, object>, bool> filter, CancellationToken cancellationToken = default) where T : IEvent
        {
            RemoveExpiredEvents();

            lock (_lockObj)
            {
                var matchingRecordedEvent = _events
                    .Where(x => x.Event is T)
                    .FirstOrDefault(recordedEvent => filter((T)recordedEvent.Event, recordedEvent.Arguments));

                if (matchingRecordedEvent != null)
                {
                    _log.Debug("Matching event for {eventType} returned from event store with age of {eventAge} ms", typeof(T).FullName, (_clock.UtcNow - matchingRecordedEvent.TimeStamp).TotalMilliseconds);
                    return Task.FromResult((T)matchingRecordedEvent.Event);
                }
            }

            var task = new WaitTask((evnt, args) => filter((T)evnt, args), typeof(T));

            cancellationToken.Register(() =>
            {
                _log.Debug("Awaiting event on task {awaiterTaskId} cancelled for event {eventType}", task.GetHashCode(), typeof(T).FullName);
                task.WaitHandle.Set();
            });

            _log.Debug("Creating event awaiter task {awaiterTaskId} for event {eventType}", task.GetHashCode(), typeof(T).FullName);

            lock (_lockObj) { _tasks.Add(task); }

            return new TaskFactory<T>().StartNew(() =>
            {
                task.WaitHandle.WaitOne();

                _log.Verbose("Removing awaiting event task {awaiterTaskId} for event {eventType}", task.GetHashCode(), typeof(T).FullName);

                lock (_lockObj) { _tasks.Remove(task); }

                return (T)task.Event;

            }, cancellationToken);
        }

        private class WaitTask
        {
            public Func<IEvent, IDictionary<string, object>, bool> EventFilter { get; }
            public Type EventType { get; }
            public EventWaitHandle WaitHandle { get; }
            public IEvent Event { get; set; }

            public WaitTask(Func<IEvent, IDictionary<string, object>, bool> eventFilter, Type eventType)
            {
                EventFilter = eventFilter;
                EventType = eventType;
                WaitHandle = new ManualResetEvent(false);
            }
        }
    }
}
