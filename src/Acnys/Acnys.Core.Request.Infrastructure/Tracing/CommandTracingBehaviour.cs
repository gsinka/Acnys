using Acnys.Core.Request.Abstractions;
using Acnys.Core.Services;
using OpenTracing;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Acnys.Core.Request.Infrastructure.Tracing
{
    public class CommandTracingBehaviour<TCommand> : IHandleCommand<TCommand> where TCommand : ICommand
    {
        private readonly ILogger _log;
        private readonly IHandleCommand<TCommand> _nextCommand;
        private readonly ITracer _tracer;

        public CommandTracingBehaviour(ILogger log, IHandleCommand<TCommand> nextCommand, ITracer tracer)
        {
            _log = log;
            _nextCommand = nextCommand;
            _tracer = tracer;
        }
        public async Task Handle(TCommand command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            var spanbuilder = _tracer.BuildSpan(typeof(TCommand).Name);
            using var span = spanbuilder.StartActive(true);
            foreach (var arg in arguments)
            {
                span.Span.SetBaggageItem(arg.Key, arg.Value.ToString());
                span.Span.SetTag(new OpenTracing.Tag.StringTag(arg.Key), arg.Value.ToString());
            }
            Log.Verbose("Command span started");
            await _nextCommand.Handle(command, arguments, cancellationToken);
            Log.Verbose("Command span ended");
        }
    }
}
