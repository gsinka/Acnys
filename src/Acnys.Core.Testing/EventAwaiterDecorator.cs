using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Eventing;
using Acnys.Core.Request.Application;
using Autofac.Features.Decorators;
using Serilog;

namespace Acnys.Core.Testing
{
    public class EventAwaiterDecorator<T> : IHandleEvent<T> where T : IEvent
    {
        private readonly ILogger _log;
        private readonly EventAwaiter<T> _testHelper;
        private readonly IHandleEvent<T> _eventHandler;
        private readonly IDecoratorContext _decoratorContext;

        public EventAwaiterDecorator(ILogger log, EventAwaiter<T> testHelper, IHandleEvent<T> eventHandler, IDecoratorContext decoratorContext)
        {
            _log = log;
            _testHelper = testHelper;
            _eventHandler = eventHandler;
            _decoratorContext = decoratorContext;
        }

        public async Task Handle(T evnt, CancellationToken cancellationToken = default)
        {
            _testHelper.ProcessEvent(evnt);
            await _eventHandler.Handle(evnt, cancellationToken);
        }
    }
}
