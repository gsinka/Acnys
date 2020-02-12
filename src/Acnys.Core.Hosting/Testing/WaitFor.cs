using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Eventing;
using Acnys.Core.Request.Application;
using Autofac;
using Autofac.Features.Decorators;
using Serilog;

namespace Acnys.Core.Hosting.Testing
{
    public class AwaiterTask<T> : IDisposable
    {
        public T Event { get; set; }
        public Func<T, bool> Filter { get; }
        public ManualResetEvent ResetEvent { get; }

        public AwaiterTask(Func<T, bool> filter, ManualResetEvent resetEvent)
        {
            Filter = filter;
            ResetEvent = resetEvent;
        }

        public void Dispose()
        {
        }
    }

    public class TestHelper<T>
    {
        private readonly ILifetimeScope _rootScope;
        private List<AwaiterTask<T>> _tasks = new List<AwaiterTask<T>>();
        
        public TestHelper(ILifetimeScope rootScope)
        {
            _rootScope = rootScope;
        }

        public async Task ProcessEvent(T evnt)
        {
            foreach (var task in _tasks)
            {
                if (task.Filter(evnt))
                {
                    task.Event = evnt;
                    task.ResetEvent.Set();
                }
            }
        }

        public async Task<T> WaitFor(Func<T, bool> filter)
        {
            using var task = new AwaiterTask<T>(filter, new ManualResetEvent(false));
            _tasks.Add(task);
            task.ResetEvent.WaitOne(TimeSpan.FromSeconds(1));
            return (T)task.Event;
        }
    }

    public class EventAwaiter<T> : IHandleEvent<T> where T : IEvent
    {
        private readonly ILogger _log;
        private readonly TestHelper<T> _testHelper;
        private readonly IHandleEvent<T> _eventHandler;
        private readonly IDecoratorContext _decoratorContext;

        public EventAwaiter(ILogger log, TestHelper<T> testHelper, IHandleEvent<T> eventHandler, IDecoratorContext decoratorContext)
        {
            _log = log;
            _testHelper = testHelper;
            _eventHandler = eventHandler;
            _decoratorContext = decoratorContext;
        }

        public async Task Handle(T evnt, CancellationToken cancellationToken = default)
        {
            _log.Debug("Event awaiter checking registered awaiters");
            await _testHelper.ProcessEvent(evnt);
            await _eventHandler.Handle(evnt, cancellationToken);
        }

        public async Task<T> WaitFor(Func<T, bool> filter, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return default;
        }
    }


    //public class TestHelper
    //{
    //    private readonly ILifetimeScope _scope;

    //    public TestHelper(ILifetimeScope scope)
    //    {
    //        _scope = scope;
    //    }

    //    public Task<TEvent> SendCommandAndWaitForEvent<TEvent, TCommand>(TCommand command,
    //        Func<TEvent, bool> filter, TimeSpan timeout, CancellationToken cancellationToken = default)
    //        where TEvent : IEvent 
    //        where TCommand : ICommand
    //    {
    //        var manualResetEvent = new ManualResetEvent(false);
    //        var handler = new TestEventHandler<TEvent>(manualResetEvent, filter);
            
    //        using var scope = _scope.BeginLifetimeScope(builder => builder.RegisterInstance(handler).As<IHandleEvent<TEvent>>());
    //        scope.Resolve<ISendCommand>().Send(command, cancellationToken);

    //        return Task.FromResult((TEvent) (manualResetEvent.WaitOne(timeout) ? handler.Event : null));
    //    }

    //    public class TestEventHandler<T> : IHandleEvent<T> where T : IEvent
    //    {
    //        private readonly ManualResetEvent _manualResetEvent;
    //        private readonly Func<T, bool> _filter;
    //        public IEvent Event;

    //        public TestEventHandler(ManualResetEvent manualResetEvent, Func<T, bool> filter)
    //        {
    //            _manualResetEvent = manualResetEvent;
    //            _filter = filter;
    //        }

    //        public async Task Handle(T evnt, CancellationToken cancellationToken = default)
    //        {
    //            if (_filter(evnt))
    //            {
    //                Event = evnt;
    //                _manualResetEvent.Set();
    //            }
    //        }

    //        public async Task<T> WaitOne(Func<T, bool> filter, TimeSpan timeout)
    //        {

    //        }
    //    }
    //}
}
