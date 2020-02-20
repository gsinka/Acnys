using System.Collections.Generic;
using Autofac;
using Serilog;

namespace Acnys.Core.RabbitMQ.Extensions
{
    public class RabbitAutoStartService : IStartable
    {
        private readonly ILifetimeScope _scope;
        private readonly ILogger _log;

        public RabbitAutoStartService(ILifetimeScope scope, ILogger log)
        {
            _scope = scope;
            _log = log;
        }
        
        public void Start()
        {
            _log.Debug("Auto starting event listeners");
            foreach (var eventListener in _scope.Resolve<IEnumerable<RabbitEventListener>>())
            {
                _log.Verbose("Starting listener for queue {queue} (consumer tag: {consumerTag}", eventListener.Queue, eventListener.ConsumerTag);
                eventListener.Start();
            }
        }
    }
}