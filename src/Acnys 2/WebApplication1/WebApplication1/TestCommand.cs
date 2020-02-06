using System;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core;
using Acnys.Core.Request;
using Acnys.Core.Request.Abstractions;

namespace WebApplication1
{
    public class TestCommand : Command
    {
    }

    public class TestCommandHandler : IHandleCommand<TestCommand>
    {
        public Task Handle(TestCommand command, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
