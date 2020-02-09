using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client.Events;

namespace Acnys.Core.Hosting.RabbitMQ
{
    public class RabbitConsumerHealtCheck : IHealthCheck
    {
        private readonly EventingBasicConsumer _consumer;

        public RabbitConsumerHealtCheck(EventingBasicConsumer consumer)
        {
            _consumer = consumer;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.Run(() => new HealthCheckResult(_consumer.IsRunning ? HealthStatus.Healthy : HealthStatus.Unhealthy, $"RabbitMQ Consumer {_consumer.ConsumerTag}", null, new Dictionary<string, object>()), cancellationToken);
        }
    }
}