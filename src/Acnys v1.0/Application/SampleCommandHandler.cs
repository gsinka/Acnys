using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Application.Abstractions;
using Acnys.Core.Domain;
using Acnys.Core.Helper;
using Api;
using Domain;

namespace Application
{
    public class SampleCommandHandler : IHandleCommand<SampleCommand>
    {
        private readonly IPublishEvent _eventPublisher;
        private readonly IAggregateRepository<SampleAggregateRoot> _repository;

        public SampleCommandHandler(IPublishEvent eventPublisher, IAggregateRepository<SampleAggregateRoot> repository)
        {
            _eventPublisher = eventPublisher;
            _repository = repository;
        }

        public async Task Handle(SampleCommand command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            await _repository.Execute(command.Id, aggregate => aggregate.DoSomething());
        }
    }
}
