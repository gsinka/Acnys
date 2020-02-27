using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.RabbitMQ;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Acnys.Core.AspNet.RabbitMQ
{
    public class RabbitEventListenerHealthCheck : IHealthCheck
    {
        private readonly IEnumerable<RabbitEventListener> _listeners;

        public RabbitEventListenerHealthCheck(IEnumerable<RabbitEventListener> listeners)
        {
            _listeners = listeners;
        }
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            var consumerState = _listeners.ToDictionary(listener => listener.ConsumerTag, listener => listener.Consumer.IsRunning);

            return Task.FromResult(new HealthCheckResult(
                consumerState.Values.All(x => x) ? HealthStatus.Healthy : HealthStatus.Unhealthy,
                $"{consumerState.Count(pair => !pair.Value)} of {consumerState.Count} consumers are not running",
                null,
                consumerState.ToDictionary(pair => pair.Key, pair => pair.Value ? "Running" : (object) "Stopped")));
        }
    }
}
