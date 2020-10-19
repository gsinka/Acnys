using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Request.Abstractions;
using Serilog;

namespace Acnys.Core.Correlation
{
    public class QuerySenderCorrelationBehavior : ISendQuery
    {
        private readonly ILogger _log;
        private readonly CorrelationContext _correlationContext;
        private readonly ISendQuery _next;

        public QuerySenderCorrelationBehavior(ILogger log, CorrelationContext correlationContext, ISendQuery next)
        {
            _log = log;
            _correlationContext = correlationContext;
            _next = next;
        }

        public async Task<T> Send<T>(IQuery<T> query, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            arguments.UseCorrelationId(_correlationContext.CorrelationId);
            arguments.UseCausationId(_correlationContext.CausationId);
            _log.Debug("Updating arguments from Correlation context {contextId}: {@context}", _correlationContext.GetHashCode(), _correlationContext);

            return  await _next.Send(query, arguments, cancellationToken);

        }
    }
}