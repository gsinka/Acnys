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
    public class CommandDurationMetricBehaviour<TCommand> : IHandleCommand<TCommand> where TCommand : ICommand
    {
        private readonly ILogger _log;
        private readonly IHandleCommand<TCommand> _nextCommand;
        private readonly MetricsService _metricsService;
        private readonly IDecoratorContext _decoratorContext;

        public CommandDurationMetricBehaviour(ILogger log, IHandleCommand<TCommand> nextCommand, MetricsService metricsService, IDecoratorContext decoratorContext)
        {
            _log = log;
            _nextCommand = nextCommand;
            _metricsService = metricsService;
            _decoratorContext = decoratorContext;
        }

        public async Task Handle(TCommand command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            var sw = new Stopwatch();
            _log.Verbose("Command count metric incremented");
            sw.Restart();
            await _nextCommand.Handle(command, arguments, cancellationToken);
            sw.Stop();
            _metricsService.PutObservationInSecond(
                _decoratorContext.ImplementationType.Namespace, _decoratorContext.ImplementationType.Name, command.GetType().Namespace, command.GetType().Name, sw.Elapsed.TotalSeconds,"Command Handler");
        }
    }
}
