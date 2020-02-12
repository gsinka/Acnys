using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Request;
using Acnys.Core.Request.Application;
using Serilog;

namespace Acnys.Core.Hosting.Request.Sender
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

        public async Task Send<T>(T command, CancellationToken cancellationToken = default) where T : ICommand
        {
            _log.Debug("Sending command to loopback dispatcher");

            await _commandDispatcher.Dispatch(command, cancellationToken);
        }

        public async Task<T> Send<T>(IQuery<T> query, CancellationToken cancellationToken = default)
        {
            _log.Debug("Sending query to loopback dispatcher");

            return await _queryDispatcher.Dispatch(query, cancellationToken);
        }
    }
}