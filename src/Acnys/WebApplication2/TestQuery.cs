using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Request;
using Acnys.Core.Request.Abstractions;

namespace WebApplication2
{
    public class TestQuery : Query<string>
    {
        public string Data { get; }

        public TestQuery(string data)
        {
            Data = data;
        }
    }

    public class TestQueryHandler : IHandleQuery<TestQuery, string>
    {
        public Task<string> Handle(TestQuery query, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(query.Data);
        }
    }
}