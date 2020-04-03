using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Request.Abstractions;
using Serilog;

namespace Acnys.Core.Request.Infrastructure.Senders
{
    public class LoopbackRequestSender : ISendRequest
    {
        private readonly ILogger _log;
        private readonly IDispatchCommand _commandDispatcher;
        private readonly IDispatchQuery _queryDispatcher;

        public LoopbackRequestSender(ILogger log, IDispatchCommand commandDispatcher, IDispatchQuery queryDispatcher)
        {
            _log = log;
            _commandDispatcher = commandDispatcher;
            _queryDispatcher = queryDispatcher;
        }

        public async Task Send<T>(T command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default) where T : ICommand
        {
            _log.Debug("Sending command to loopback dispatcher");
            
            _log.Verbose("Command data: {@command}", command);
            _log.Verbose("Command arguments: {@arguments}", arguments);


            await _commandDispatcher.Dispatch(command, arguments, cancellationToken);
        }

        public async Task<T> Send<T>(IQuery<T> query, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            _log.Debug("Sending query to loopback dispatcher");

            _log.Verbose("Query data: {@query}", query);
            _log.Verbose("Query arguments: {@query}", arguments);

            return await _queryDispatcher.Dispatch(query, arguments, cancellationToken);
        }
    }
}