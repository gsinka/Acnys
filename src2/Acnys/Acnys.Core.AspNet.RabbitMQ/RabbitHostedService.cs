using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Eventing.Abstractions;
using Acnys.Core.RabbitMQ;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Serilog;

namespace Acnys.Core.AspNet.RabbitMQ
{
    public class RabbitHostedService : BackgroundService, IRabbitService
    {
        private readonly IConnection _connection;
        private readonly RabbitServiceConfiguration _serviceConfiguration;
        private readonly IRabbitService _internal;


        public RabbitHostedService(ILogger log, IDispatchEvent eventDispatcher, IOptions<RabbitServiceConfiguration> serviceConfiguration)
        {
            _serviceConfiguration = serviceConfiguration.Value;

            _connection = new ConnectionFactory()
            {
                Uri = new Uri(_serviceConfiguration.Uri),
                AutomaticRecoveryEnabled = true,

            }.CreateConnection();

            _internal = new RabbitService(log.ForContext<RabbitService>(), _connection, eventDispatcher, _serviceConfiguration.Publisher.Exchange);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Create listeners
            foreach (var listenerConfig in _serviceConfiguration.Listeners)
            {
                _internal.AddEventListener(listenerConfig.Queue, listenerConfig.ConsumerTag, listenerConfig.ConsumerArguments);
            }

            return Task.CompletedTask;
        }

        public async Task Publish<T>(T @event, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default) where T : IEvent
        {
            await _internal.Publish(@event, arguments, cancellationToken);
        }

        public void CreateExchange(string name, string type = ExchangeType.Fanout, bool durable = false, bool autoDelete = false, IDictionary<string, object> arguments = null)
        {
            _internal.CreateExchange(name, type, durable, autoDelete);
        }

        public void CreateQueue(string name, bool durable = false, bool exclusive = false, bool autoDelete = false, IDictionary<string, object> arguments = null)
        {
            _internal.CreateQueue(name, durable, exclusive, autoDelete, arguments);
        }

        public void Bind(string queue, string exchange, string routingKey = "", IDictionary<string, object> arguments = null)
        {
            _internal.Bind(queue, exchange, routingKey, arguments);
        }

        public void AddEventListener(string queue, string consumerTag = null, IDictionary<string, object> arguments = null)
        {
            _internal.AddEventListener(queue, consumerTag, arguments);
        }
    }
}