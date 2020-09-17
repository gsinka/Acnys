using Acnys.Core.Request.Abstractions;
using Acnys.Core.Services;
using Prometheus;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Acnys.Core.Request.Infrastructure.Metrics
{
    public class CommandCountMetricBehaviour<TCommand> : IHandleCommand<TCommand> where TCommand : ICommand
    {
        private readonly ILogger _log;
        private readonly IHandleCommand<TCommand> _nextCommand;
        private readonly MetricsService _metricsService;

        public CommandCountMetricBehaviour(ILogger log, IHandleCommand<TCommand> nextCommand, MetricsService metricsService)
        {
            _log = log;
            _nextCommand = nextCommand;
            _metricsService = metricsService;
        }
        public async Task Handle(TCommand command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            _metricsService.IncrementCounter<TCommand>(1, "CommandHandlerCounter");
            _log.Verbose("Command count metric incremented");
            await _nextCommand.Handle(command, arguments, cancellationToken);
        }
    }
}
