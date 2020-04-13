using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Application.Abstractions;
using Api;
using Api.Events;

namespace Application
{
    public class SampleEventHandler : IHandleEvent<SampleEvent>
    {
        public Task Handle(SampleEvent @event, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
