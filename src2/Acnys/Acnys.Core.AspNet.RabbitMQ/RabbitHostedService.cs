using System;
using System.Collections.Generic;
using System.Net.Http;
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
    public class RabbitHostedService : BackgroundService, IPublishEvent
    {
        private readonly ILogger _log;
        private readonly IConnection _connection;
        private HttpClientHandler _clientHandler = new HttpClientHandler();
        private readonly IDispatchEvent _eventDispatcher;
        private readonly RabbitServiceConfiguration _serviceConfiguration;
        private EventPublisher _publisher;
        private List<EventListener> _listeners = new List<EventListener>();


        public RabbitHostedService(ILogger log, IDispatchEvent eventDispatcher, IOptions<RabbitServiceConfiguration> serviceConfiguration)
        {
            _log = log;
            _eventDispatcher = eventDispatcher;
            _serviceConfiguration = serviceConfiguration.Value;

            _connection = new ConnectionFactory()
            {
                Uri = new Uri(_serviceConfiguration.Uri),
                AutomaticRecoveryEnabled = true,

            }.CreateConnection();

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Create publisher
            _publisher = new EventPublisher(_log.ForContext<EventPublisher>(), _connection, _serviceConfiguration.Publisher.Exchange, EventPublisher.DefaultContextBuilder);

            // Create listeners
            foreach (var listenerConfig in _serviceConfiguration.Listeners)
            {
                _listeners.Add(new EventListener(
                    _log.ForContext<EventListener>(), 
                    _connection, _eventDispatcher, 
                    listenerConfig.Queue, 
                    listenerConfig.ConsumerTag, 
                    listenerConfig.ConsumerArguments, 
                    EventListener.Default));
            }
        }

        public async Task Publish<T>(T @event, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default) where T : IEvent
        {
            await _publisher.Publish(@event, arguments, cancellationToken);
        }
    }
}