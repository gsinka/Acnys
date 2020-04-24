using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Abstractions;
using Acnys.Core.Application.Abstractions;

namespace Application
{
    public class SampleAllEventHandler : IHandleEvent
    {
        public Task Handle(IEvent @event, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}