using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Request.Abstractions;

namespace Acnys.Core.Request.Tests
{
    public class TestCommand : Command
    {
        public TestCommand(Guid? requestId = null, Guid? causationId = null, Guid? correlationId = null) : base(requestId, causationId, correlationId)
        {
        }
    }

    public class TestCommandHandler : IHandleCommand<TestCommand>
    {
        public Task Handle(TestCommand command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}