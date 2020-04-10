using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Request.Abstractions;

namespace Acnys.Core.Request.Tests
{
    public class TestCommandHandler : IHandleCommand<TestCommand>
    {
        public IDictionary<string, object> Arguments { get; private set; }

        public TestCommand Command { get; private set; }

        public Task Handle(TestCommand command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            Arguments = arguments;
            Command = command;

            return Task.CompletedTask;
        }
    }
}