using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Request.Abstractions;
using Microsoft.Extensions.Logging;

namespace Acnys.Request.Senders
{
    /// <summary>
    /// Loopback command sender
    /// </summary>
    /// <remarks>Loopback command sender is using <see cref="IDispatchCommand"/> to dispatch command locally</remarks>
    public class LoopbackCommandSender : ISendCommand
    {
        private readonly ILogger<LoopbackCommandSender> _log;
        private readonly IDispatchCommand _commandDispatcher;

        public LoopbackCommandSender(ILogger<LoopbackCommandSender> log, IDispatchCommand commandDispatcher)
        {
            _log = log;
            _commandDispatcher = commandDispatcher;

            _log.LogTrace("Initiated loopback command sender {senderId}", GetHashCode());
        }

        public async Task SendAsync<T>(T command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default) where T : ICommand
        {
            using var logScope = _log.BeginScope("Dispatch");
            
            _log.LogTrace("Dispatching command {commandType}", command.GetType().Name);
            await _commandDispatcher.DispatchAsync(command, arguments, cancellationToken);
        }
    }
}