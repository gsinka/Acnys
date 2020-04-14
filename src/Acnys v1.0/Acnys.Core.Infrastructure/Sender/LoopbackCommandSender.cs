using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Abstractions;
using Acnys.Core.Application.Abstractions;
using Acnys.Core.Infrastructure.Dispatcher;
using Acnys.Core.Infrastructure.Serilog;
using Serilog;

namespace Acnys.Core.Infrastructure.Sender
{
    public class LoopbackCommandSender : ISendCommand
    {
        private readonly ILogger _log;
        private readonly IDispatchCommand _commandDispatcher;

        public LoopbackCommandSender(ILogger log, IDispatchCommand commandDispatcher)
        {
            _log = log;
            _commandDispatcher = commandDispatcher;
        }

        public async Task Send<T>(T command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default) where T : ICommand
        {
            arguments.EnrichLog();

            _log.Debug("Sending command to loopback dispatcher");
            await _commandDispatcher.Dispatch(command, arguments, cancellationToken);
        }
    }
}
