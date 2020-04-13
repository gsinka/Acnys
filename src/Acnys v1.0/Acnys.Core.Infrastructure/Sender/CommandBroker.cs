using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Abstractions;
using Acnys.Core.Application.Abstractions;
using Autofac.Features.Indexed;
using Serilog;

namespace Acnys.Core.Infrastructure.Sender
{
    public class CommandBroker : ISendCommand
    {
        private readonly ILogger _log;
        private readonly Func<ICommand, IDictionary<string, object>, object> _senderKeySelector;
        private readonly IIndex<object, ISendCommand> _senderResolver;


        public CommandBroker(ILogger log, Func<ICommand, IDictionary<string, object>, object> senderKeySelector, IIndex<object, ISendCommand> senderResolver)
        {
            _log = log;
            _senderKeySelector = senderKeySelector;
            _senderResolver = senderResolver;
        }

        public async Task Send<T>(T command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default) where T : ICommand
        {
            arguments.EnrichLog();

            var senderKey = _senderKeySelector(command, arguments);

            if (senderKey == null)
            {
                throw new InvalidOperationException($"Unable to resolve sender key for command {command.GetType().FullName}");
            }

            _log.Verbose("Sender key {senderKey} resolved for command {commandType}", senderKey, command.GetType().Name);

            
            if (!_senderResolver.TryGetValue(senderKey, out var sender))
            {
                throw new InvalidOperationException($"Unable to resolve sender for key {senderKey}");
            }

            _log.Verbose("Sender {senderType} resolved for command {commandType}", sender.GetType().Name, command.GetType().Name);
            
            await sender.Send(command, arguments, cancellationToken);
        }
    }
}
