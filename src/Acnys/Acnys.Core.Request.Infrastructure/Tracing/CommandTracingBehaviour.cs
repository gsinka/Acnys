using Acnys.Core.Attributes;
using Acnys.Core.Extensions;
using Acnys.Core.Request.Abstractions;
using Autofac.Features.Decorators;
using Microsoft.Extensions.Primitives;
using OpenTracing;
using OpenTracing.Propagation;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Acnys.Core.Request.Infrastructure.Tracing
{
    public class CommandTracingBehaviour<TCommand> : IHandleCommand<TCommand> where TCommand : ICommand
    {
        private readonly ILogger _log;
        private readonly IHandleCommand<TCommand> _nextCommand;
        private readonly ITracer _tracer;
        private readonly IDecoratorContext _decoratorContext;

        public CommandTracingBehaviour(ILogger log, IHandleCommand<TCommand> nextCommand, ITracer tracer, IDecoratorContext decoratorContext)
        {
            _log = log;
            _nextCommand = nextCommand;
            _tracer = tracer;
            _decoratorContext = decoratorContext;
        }
        public async Task Handle(TCommand command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            var triggerInfo = command.GetType().GetCustomAttributes(false).OfType<HumanReadableInformationAttribute>().FirstOrDefault();
            var handlerInfo = _decoratorContext.ImplementationType.GetCustomAttributes(false).OfType<HumanReadableInformationAttribute>().FirstOrDefault();

            var span2 = TracingExtensions.StartNewSpanForHandler(_tracer, _decoratorContext.ImplementationType.Namespace, _decoratorContext.ImplementationType.Name, command.GetType().Namespace, command.GetType().Name, handlerInfo, triggerInfo, arguments);

            Log.Verbose("Event span started");
            await _nextCommand.Handle(command, arguments, cancellationToken);
            span2.Finish();
            Log.Verbose("Event span ended");
        }
    }
}
