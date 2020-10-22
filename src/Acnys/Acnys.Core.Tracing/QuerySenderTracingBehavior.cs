using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Request.Abstractions;
using Serilog;

namespace Acnys.Core.Tracing
{
    public class QuerySenderTracingBehavior : ISendQuery
    {
        private readonly ILogger _log;
        private readonly TracingContext _tracingContext;
        private readonly ISendQuery _next;

        public QuerySenderTracingBehavior(ILogger log, TracingContext tracingContext, ISendQuery next)
        {
            _log = log;
            _tracingContext = tracingContext;
            _next = next;
        }

        public async Task<T> Send<T>(IQuery<T> query, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            arguments.UpdateWithTracingContext(_log, _tracingContext);
            return  await _next.Send(query, arguments, cancellationToken);
        }
    }
}