using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Abstractions;
using Acnys.Core.Application.Abstractions;
using Acnys.Core.Infrastructure.Dispatcher;
using Acnys.Core.Infrastructure.Serilog;
using Serilog;

namespace Acnys.Core.Infrastructure.Sender
{
    public class LoopbackQuerySender : ISendQuery
    {
        private readonly ILogger _log;
        private readonly IDispatchQuery _queryDispatcher;

        public LoopbackQuerySender(ILogger log, IDispatchQuery queryDispatcher)
        {
            _log = log;
            _queryDispatcher = queryDispatcher;
        }

        public async Task<T> Send<T>(IQuery<T> query, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            arguments.EnrichLog();

            _log.Debug("Sending query to loopback dispatcher");
            return await _queryDispatcher.Dispatch(query, arguments, cancellationToken);
        }
    }
}