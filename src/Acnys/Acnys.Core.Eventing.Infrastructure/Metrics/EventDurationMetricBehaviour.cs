using Acnys.Core.Eventing.Abstractions;
using Acnys.Core.Services;
using Autofac.Features.Decorators;
using Serilog;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Acnys.Core.Eventing.Infrastructure.Metrics
{
    public class EventDurationMetricBehaviour<TEvent> : IHandleEvent<TEvent> where TEvent : IEvent
    {
        private readonly ILogger _log;
        private readonly IHandleEvent<TEvent> _nextEvent;
        private readonly MetricsService _metricsService;
        private readonly IDecoratorContext _decoratorContext;

        public EventDurationMetricBehaviour(ILogger log, IHandleEvent<TEvent> nextCommand, MetricsService metricsService, IDecoratorContext decoratorContext)
        {
            _log = log;
            _nextEvent = nextCommand;
            _metricsService = metricsService;
            _decoratorContext = decoratorContext;
        }

        public async Task Handle(TEvent command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {   
            var sw = new Stopwatch();
            _log.Verbose("Event count metric incremented");
            sw.Restart();
            await _nextEvent.Handle(command, arguments, cancellationToken);
            sw.Stop();
            _metricsService.PutObservationInSecond(_decoratorContext.ImplementationType.Namespace, _decoratorContext.ImplementationType.Name, command.GetType().Namespace, command.GetType().Name, sw.Elapsed.TotalSeconds, "Event Handler");
        }
    }
}
