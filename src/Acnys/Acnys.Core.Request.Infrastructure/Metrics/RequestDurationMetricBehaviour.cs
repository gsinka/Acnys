using Acnys.Core.Request.Abstractions;
using Acnys.Core.Services;
using Autofac.Features.Decorators;
using Prometheus;
using Serilog;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Acnys.Core.Request.Infrastructure.Metrics
{
    public class RequestDurationMetricBehaviour<TQuery,TResult> : IHandleQuery<TQuery,TResult> where TQuery : IQuery<TResult>
    {
        private readonly IHandleQuery<TQuery,TResult> _nextCommand;
        private readonly MetricsService _metricsService;
        private readonly IDecoratorContext _decoratorContext;

        public RequestDurationMetricBehaviour(IHandleQuery<TQuery, TResult> nextCommand, MetricsService metricsService, IDecoratorContext decoratorContext)
        {
            _nextCommand = nextCommand;
            _metricsService = metricsService;
            _decoratorContext = decoratorContext;
        }

        public async Task<TResult> Handle(TQuery query, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            var sw = new Stopwatch();
            sw.Restart();
            var response = await _nextCommand.Handle(query, arguments, cancellationToken);
            sw.Stop();
            _metricsService.PutObservationInSecond(
                _decoratorContext.ImplementationType.Namespace, _decoratorContext.ImplementationType.Name, query.GetType().Namespace, query.GetType().Name, sw.Elapsed.TotalSeconds, "Query Handler");
            return response;
        }
    }
}
