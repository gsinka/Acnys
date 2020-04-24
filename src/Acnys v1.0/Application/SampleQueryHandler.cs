using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Application.Abstractions;
using Api;

namespace Application
{
    public class SampleQueryHandler : IHandleQuery<SampleQuery, SampleQuery.Result>
    {
        public Task<SampleQuery.Result> Handle(SampleQuery query, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}