using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Request.Abstractions;
using Serilog;

namespace Acnys.Core.Correlation
{
    public class QueryCorrelationBehavior : IDispatchQuery
    {
        private readonly ILogger _log;
        private readonly CorrelationContext _correlationContext;
        private readonly IDispatchQuery _next;
        
        public QueryCorrelationBehavior(ILogger log, CorrelationContext correlationContext, IDispatchQuery next)
        {
            _log = log;
            _correlationContext = correlationContext;
            _next = next;
        }

        public async Task<TResult> Dispatch<TResult>(IQuery<TResult> query, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            _correlationContext.CorrelationId = arguments?.CorrelationId() ?? Guid.NewGuid();
            _correlationContext.CausationId = query.RequestId;
            _log.Debug("Correlation context {contextId} updated: {@context}", _correlationContext.GetHashCode(), _correlationContext);

            return await _next.Dispatch(query, arguments, cancellationToken);

        }
    }
}