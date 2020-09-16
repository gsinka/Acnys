using Acnys.Core.Request.Abstractions;
using FluentValidation;
using Prometheus;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Acnys.Core.Request.Infrastructure.Metrics
{
    public class CommandMetrics<TCommand> : IHandleCommand<TCommand> where TCommand : ICommand
    {
        private readonly ILogger _log;
        private readonly IHandleCommand<TCommand> _commandHandler;

        private static readonly List<Counter> _counters = new List<Counter>();
        private Counter counterMetric;
        private readonly string _counterMetricsName;

        public CommandMetrics(ILogger log, IHandleCommand<TCommand> commandHandler)
        {
            _log = log;
            _commandHandler = commandHandler;
            _counterMetricsName = "command_handler_" + typeof(TCommand).FullName.ToLower().Replace('.', '_');
            counterMetric = _counters.FirstOrDefault(c => c.Name == _counterMetricsName);
            if (counterMetric == null)
            {
                counterMetric = Prometheus.Metrics.CreateCounter(_counterMetricsName, "auto generated command handler counter");
                _counters.Add(counterMetric);
            }
        }
        public async Task Handle(TCommand command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            counterMetric.Inc();
            _log.Information("Command metrics started");
            await _commandHandler.Handle(command, arguments, cancellationToken);
            _log.Information("Command metrics ended");
        }
    }
}
