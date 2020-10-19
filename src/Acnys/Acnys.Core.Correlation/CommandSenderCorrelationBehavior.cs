using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Eventing.Abstractions;
using Acnys.Core.Request.Abstractions;
using Serilog;

namespace Acnys.Core.Correlation
{
    public class CommandSenderCorrelationBehavior : ISendCommand
    {
        private readonly ILogger _log;
        private readonly CorrelationContext _correlationContext;
        private readonly ISendCommand _next;

        public CommandSenderCorrelationBehavior(ILogger log, CorrelationContext correlationContext, ISendCommand next)
        {
            _log = log;
            _correlationContext = correlationContext;
            _next = next;
        }

        public async Task Send<T>(T command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default) where T : ICommand
        {
            arguments.UseCorrelationId(_correlationContext.CorrelationId);
            arguments.UseCausationId(_correlationContext.CausationId);
            _log.Debug("Updating arguments from Correlation context {contextId}: {@context}", _correlationContext.GetHashCode(), _correlationContext);

            await _next.Send(command, arguments, cancellationToken);
        }
    }
}