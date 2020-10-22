using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Request.Abstractions;
using Autofac.Features.Decorators;
using Serilog;

namespace Acnys.Core.Tracing
{
    public class CommandSenderTracingBehavior : ISendCommand
    {
        private readonly ILogger _log;
        private readonly TracingContext _tracingContext;
        private readonly ISendCommand _next;
        private readonly IDecoratorContext _context;

        public CommandSenderTracingBehavior(ILogger log, TracingContext tracingContext, ISendCommand next, IDecoratorContext context)
        {
            _log = log;
            _tracingContext = tracingContext;
            _next = next;
            _context = context;
        }

        public async Task Send<T>(T command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default) where T : ICommand
        {
            arguments.UpdateWithTracingContext(_log, _tracingContext);
            await _next.Send(command, arguments, cancellationToken);
        }
    }
}