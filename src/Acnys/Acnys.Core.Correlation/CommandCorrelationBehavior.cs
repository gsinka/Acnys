using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Request.Abstractions;
using Serilog;

namespace Acnys.Core.Correlation
{
    public class CommandCorrelationBehavior : IDispatchCommand
    {
        private readonly ILogger _log;
        private readonly CorrelationContext _correlationContext;
        private readonly IDispatchCommand _next;
        
        public CommandCorrelationBehavior(ILogger log, CorrelationContext correlationContext, IDispatchCommand next)
        {
            _log = log;
            _correlationContext = correlationContext;
            _next = next;
        }

        public async Task Dispatch<T>(T command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default) where T : ICommand
        {
            _correlationContext.CorrelationId = arguments?.CorrelationId() ?? Guid.NewGuid();
            _correlationContext.CausationId = command.RequestId;
            _log.Debug("Correlation context {contextId} updated: {@context}", _correlationContext.GetHashCode(), _correlationContext);

            await _next.Dispatch(command, arguments, cancellationToken);
        }
    }
}
