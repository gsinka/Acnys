using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Request.Abstractions;

namespace Acnys.Core.Request.Tests
{
    public class TestQueryHandler : IHandleQuery<TestQuery, IDictionary<string, object>>
    {
        public Task<IDictionary<string, object>> Handle(TestQuery query, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(arguments);
        }
    }
}