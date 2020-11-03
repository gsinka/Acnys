using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Request.Abstractions;
using Microsoft.Extensions.Logging;

namespace Acnys.Request.Tests
{
    public class TestCommandHandler : IHandleCommand<TestCommand>
    {
        private readonly ILogger _log;
        private readonly Action<TestCommand> _action;

        public TestCommandHandler(ILogger<TestCommandHandler> log, Action<TestCommand> action = null)
        {
            _log = log;
            _action = action;
            _log.LogDebug("Test command handler initiated");
        }

        public Task HandleAsync(TestCommand command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            _action?.Invoke(command);
            _log.LogInformation("Test command handled");
            
            return Task.CompletedTask;
        }
    }
}