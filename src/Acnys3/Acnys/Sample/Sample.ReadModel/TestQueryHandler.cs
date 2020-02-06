using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Request.ReadModel;
using Sample.Api.Facade;
using Serilog;

namespace Sample.ReadModel
{
    public class TestQueryHandler : IHandleQuery<TestQuery, string>
    {
        private readonly ILogger _log;

        public TestQueryHandler(ILogger log)
        {
            _log = log;
        }

        public Task<string> Handle(TestQuery query, CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                _log.Verbose("Query handled");
                return query.Data;
            }, cancellationToken);

        }
    }
}
