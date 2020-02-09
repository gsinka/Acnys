using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Acnys.Core.Hosting.RabbitMQ
{
    public static class RabbitConnectionHealthCheckExtensions
    {
        public static IHealthCheck CreateHealthCheck(this IConnection connection)
        {
            return new RabbitConnectionHealthCheck(connection);
        }
    }

    public static class RabbitConsumerHealthCheckExtensions
    {
        public static IHealthChecksBuilder AddRabbitConsumerHealthCheck(this IHealthChecksBuilder builder, EventingBasicConsumer consumer)
        {
            builder.Add(new HealthCheckRegistration(
                "RabbitMQ consumer", 
                provider => new RabbitConsumerHealtCheck(consumer), 
                HealthStatus.Unhealthy, 
                new List<string>() { $"ConsumerTag: {consumer.ConsumerTag}", $"ChannelNumber: {consumer.Model.ChannelNumber}" }
                ));

            return builder;
        }
    }
}