using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Application.Abstractions;

namespace Acnys.Core.CommandQuery.Test
{
    public class TestCommand : Command
    {

    }

    public class TestCommandHandler : IHandleCommand<TestCommand>
    {
        public async Task Handle(TestCommand command, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}