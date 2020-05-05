using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Request.Abstractions;
using Autofac;
using Serilog;

namespace Acnys.Core.Request.Infrastructure.Senders
{
    public class LoopbackRequestSender : ISendRequest
    {
        private readonly ILogger _log;
        private readonly ILifetimeScope _scope;

        public LoopbackRequestSender(ILogger log, ILifetimeScope scope)
        {
            _log = log;
            _scope = scope;
        }

        public async Task Send<T>(T command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default) where T : ICommand
        {
            _log.Debug("Sending command to loopback dispatcher");
            
            _log.Verbose("Command data: {@command}", command);
            _log.Verbose("Command arguments: {@arguments}", arguments);
            
            await using var scope = _scope.BeginLifetimeScope();
            _log.Debug("New lifetime scope created for command dispatcher ({scopeId})", scope.GetHashCode());

            await scope.Resolve<IDispatchCommand>().Dispatch(command, arguments, cancellationToken);

            _log.Debug("Ending lifetime scope ({scopeId})", scope.GetHashCode());
        }

        public async Task<T> Send<T>(IQuery<T> query, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            _log.Debug("Sending query to loopback dispatcher");

            _log.Verbose("Query data: {@query}", query);
            _log.Verbose("Query arguments: {@query}", arguments);

            await using var scope = _scope.BeginLifetimeScope();
            _log.Debug("New lifetime scope created for command dispatcher ({scopeId})", scope.GetHashCode());

            var result = await scope.Resolve<IDispatchQuery>().Dispatch(query, arguments, cancellationToken);
            
            _log.Debug("Ending lifetime scope ({scopeId})", scope.GetHashCode());
            return result;
        }
    }
}