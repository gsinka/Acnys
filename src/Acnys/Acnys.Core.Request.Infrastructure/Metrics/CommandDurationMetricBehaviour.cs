using Acnys.Core.Request.Abstractions;
using Acnys.Core.Services;
using Prometheus;
using Serilog;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Acnys.Core.Request.Infrastructure.Metrics
{
    public class CommandSummaryMetricBehaviour<TCommand> : IHandleCommand<TCommand> where TCommand : ICommand
    {
        private readonly ILogger _log;
        private readonly IHandleCommand<TCommand> _nextCommand;
        private readonly MetricsService _metricsService;

        public CommandSummaryMetricBehaviour(ILogger log, IHandleCommand<TCommand> nextCommand, MetricsService metricsService)
        {
            _log = log;
            _nextCommand = nextCommand;
            _metricsService = metricsService;
        }

        public async Task Handle(TCommand command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            var sw = new Stopwatch();
            _log.Verbose("Command count metric incremented");
            sw.Restart();
            await _nextCommand.Handle(command, arguments, cancellationToken);
            sw.Stop();
            _metricsService.ObserveInSummary<TCommand>(sw.Elapsed.TotalSeconds,"CommandHandlerDuration");
        }
    }
}
