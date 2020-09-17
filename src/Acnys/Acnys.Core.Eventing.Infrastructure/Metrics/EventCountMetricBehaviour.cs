 using Acnys.Core.Eventing.Abstractions;
using Acnys.Core.Services;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Acnys.Core.Eventing.Infrastructure.Metrics
{
    public class EventCountMetricBehaviour<TEvent> : IHandleEvent<TEvent> where TEvent : IEvent
    {
        private readonly ILogger _log;
        private readonly IHandleEvent<TEvent> _nextEvent;
        private readonly MetricsService _metricsService;

        public EventCountMetricBehaviour(ILogger log, IHandleEvent<TEvent> nextCommand, MetricsService metricsService)
        {
            _log = log;
            _nextEvent = nextCommand;
            _metricsService = metricsService;
        }
        public async Task Handle(TEvent command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            _metricsService.IncrementCounter<TEvent>(1, "EventHandlerCounter");
            _log.Verbose("Event count metric incremented");
            await _nextEvent.Handle(command, arguments, cancellationToken);
        }
    }
}
