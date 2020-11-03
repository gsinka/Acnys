using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Request.Abstractions;
using Acnys.Request.Senders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Acnys.Request.Behaviors
{
    public class LoopbackSenderScopeBehavior : ISendCommand
    {
        private readonly ILogger<LoopbackSenderScopeBehavior> _log;
        private readonly IServiceProvider _serviceProvider;
        private readonly CommandFilterDelegate _filter;
        private readonly Func<ISendCommand> _next;

        public LoopbackSenderScopeBehavior(ILogger<LoopbackSenderScopeBehavior> log, IServiceProvider serviceProvider, CommandFilterDelegate filter, Func<ISendCommand> next)
        {
            _log = log;
            _serviceProvider = serviceProvider;
            _filter = filter;
            _next = next;
        }

        public async Task SendAsync<T>(T command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default) where T : ICommand
        {
            if (_filter(command, arguments))
            {
                using var logScope = _log.BeginScope("Command sender scope behavior");
                using var scope = _serviceProvider.CreateScope();
                _log.LogTrace("New scope {scopeId} created for loopback command sender", scope.GetHashCode());
                
                await _next().SendAsync(command, arguments, cancellationToken);
                _log.LogTrace("Scope {scopeId} disposed", scope.GetHashCode());
            }

        }
    }
}
