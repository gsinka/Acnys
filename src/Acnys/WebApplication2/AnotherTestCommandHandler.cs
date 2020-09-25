using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Eventing.Abstractions;
using Acnys.Core.Request;
using Acnys.Core.Request.Abstractions;
using Serilog;
using WebApplication2.Commands;

namespace WebApplication2
{
    public class AnotherTestCommandHandler : IHandleCommand<AnotherTestCommand>
    {
        private readonly IPublishEvent _eventPublisher;
        private readonly ILogger _log;
        private readonly UserContext _context;

        public AnotherTestCommandHandler(IPublishEvent eventPublisher, ILogger log, UserContext context)
        {
            _eventPublisher = eventPublisher;
            _log = log;
            _context = context;
        }

        public async Task Handle(AnotherTestCommand command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}