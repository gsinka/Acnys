using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Eventing.Abstractions;
using WebApplication1;

namespace WebApplication2
{
    public class TestEventHandler : IHandleEvent<TestEvent>
    {
        public async Task Handle(TestEvent @event, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            await Task.Delay(500, cancellationToken);
        }
    }
}