using Acnys.Core.Request.Abstractions;
using Autofac.Features.Decorators;
using Microsoft.Extensions.Primitives;
using OpenTracing;
using OpenTracing.Propagation;
using OpenTracing.Tag;
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
            arguments ??= new Dictionary<string, object>();
            var test = _decoratorContext.ImplementationType;
            var headerExtractor = new TextMapExtractAdapter(arguments.ToDictionary(e => e.Key, v => v.Value.ToString()));
            var previousSpan = _tracer.Extract(BuiltinFormats.HttpHeaders, headerExtractor);
            ISpanBuilder spanbuilder;
            if (previousSpan != null)
                //command.GetType().GetCustomAttributes(false).OfType<Attribute>().First(a=>a.)
                spanbuilder = _tracer.BuildSpan(typeof(TCommand).FullName).AddReference(References.ChildOf, previousSpan);
            else
            {
                spanbuilder = _tracer.BuildSpan(typeof(TCommand).FullName);
            }
            arguments.Remove("uber-trace-id");
            var traceExtendedHeaders = arguments.ToDictionary(e => e.Key, v => v.Value.ToString());
            var headerInjector = new TextMapInjectAdapter(traceExtendedHeaders);
            using var span = spanbuilder.StartActive(true);
            _tracer.Inject(span.Span.Context, BuiltinFormats.HttpHeaders, headerInjector);
            arguments.Add("uber-trace-id", new StringValues(traceExtendedHeaders["uber-trace-id"].ToString()));
            foreach (var arg in arguments)
            {
                span.Span.SetTag(new StringTag(arg.Key), arg.Value.ToString());
            }
            //var ea = new OpenTracing.Propagation.TextMapExtractAdapter();
            Log.Verbose("Command span started");
            await _nextCommand.Handle(command, arguments, cancellationToken);
            Log.Verbose("Command span ended");
        }
    }
}
