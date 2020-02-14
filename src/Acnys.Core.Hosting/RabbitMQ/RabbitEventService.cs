using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Eventing;
using Acnys.Core.Request.Application;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;

namespace Acnys.Core.Hosting.RabbitMQ
{
    public class RabbitEventService : BackgroundService, IPublishEvent, IHealthCheck
    {
        private readonly ILogger _log;
        private readonly IDispatchEvent _eventDispatcher;
        private readonly IOptions<RabbitEventSettings> _settings;
        private readonly ConnectionFactory _connectionFactory;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly IList<EventingBasicConsumer> _consumers = new List<EventingBasicConsumer>();
        private readonly BasicPropertiesMapper _eventMapper;

        public RabbitEventService(ILogger log, IDispatchEvent eventDispatcher, IOptions<RabbitEventSettings> settings)
        {
            _log = log;
            _eventDispatcher = eventDispatcher;
            _eventMapper = new BasicPropertiesMapper(log);
            _settings = settings;

            _connectionFactory = new ConnectionFactory() {Uri = new Uri(_settings.Value.Uri), AutomaticRecoveryEnabled = true,};
            _connection = _connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            if (_settings.Value.Exchange != null)
            {
                _log.Debug("Declaring event exchange {exchangeName} (Topic)", _settings.Value.Exchange.Name);
                _channel.ExchangeDeclare(_settings.Value.Exchange.Name, _settings.Value.Exchange.Type ?? ExchangeType.Topic);

                foreach (var (queue, routingKey) in _settings.Value.Exchange.Bindings)
                {
                    _log.Debug("Binding queue {queueName} with exchange {exchangeName} using routing key '{routingKey}'",queue, _settings.Value.Exchange.Name, routingKey);
                    _channel.QueueBind(queue, _settings.Value.Exchange.Name, routingKey);
                }
            }

            if (_settings.Value.Queue != null)
            {
                _log.Debug("Declaring event queue {queueName} (durable: {durable}, exclusive: {exclusive}, auto-delete: {autoDelete})", _settings.Value.Queue.Name, _settings.Value.Queue.Durable, _settings.Value.Queue.Exclusive, _settings.Value.Queue.AutoDelete);
                _channel.QueueDeclare(_settings.Value.Queue.Name, _settings.Value.Queue.Durable, _settings.Value.Queue.Exclusive, _settings.Value.Queue.AutoDelete, _settings.Value.Queue.Arguments);

                foreach (var (exchange, routingKey) in _settings.Value.Queue.Bindings)
                {
                    _log.Debug("Binding queue {queueName} with exchange {exchangeName} using routing key '{routingKey}'", _settings.Value.Queue.Name, exchange, routingKey);
                    _channel.QueueBind(_settings.Value.Queue.Name, exchange, routingKey);
                }
            }
            //_log.Debug("Binding queue {queueName} with exchange {exchangeName} using routing key '{routingKey}'", _settings.Value.Queue.Name, _settings.Value.Exchange.Name, _settings.Value.RoutingKey);
            //_channel.QueueBind(_settings.Value.Queue.Name, _settings.Value.Exchange.Name, _settings.Value.RoutingKey);
            //_channel.BasicQos(0, 1, false);

            _log.Verbose("Creating consumers for {processorCount} processors", Environment.ProcessorCount);

            for (var core = 0; core < Environment.ProcessorCount; core++)
            {
                var consumer = new EventingBasicConsumer(_channel);
                
                consumer.Received += (sender, args) =>
                {
                    var c = sender as EventingBasicConsumer;
                    _log.Debug("Message received on consumer {consumerTag}", c.ConsumerTag);
                    
                    var evnt = _eventMapper.ToEvent(args);
                    _log.Verbose("Event data: {@evnt}", evnt);
                    
                    _eventDispatcher.Dispatch(evnt, stoppingToken);
                    c.Model.BasicAck(args.DeliveryTag, false);
                };

                consumer.Shutdown += OnConsumerShutdown;
                consumer.Registered += OnConsumerRegistered;
                consumer.Unregistered += OnConsumerUnregistered;
                consumer.ConsumerCancelled += OnConsumerConsumerCancelled;

                _channel.BasicConsume(_settings.Value.Queue.Name, false, consumer);
                _consumers.Add(consumer);

                //_log.Verbose("Consumer created with tag '{consumerTag}'", consumer.ConsumerTag);
            }

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }

        public Task Publish<T>(T evnt, CancellationToken cancellationToken = default) where T : IEvent
        {
            _log.Debug("Mapping outgoing event with BasicPropertiesMapper");
            var mapper = new BasicPropertiesMapper(_log);
            var (props, body) = mapper.FromEvent(evnt, new Dictionary<string, object>());

            _log.Debug("Publishing message with routing key '{routingKey}'", _settings.Value.RoutingKey);
            _channel.BasicPublish(_settings.Value.Exchange.Name, _settings.Value.RoutingKey, true, props, body);

            return Task.CompletedTask;
        }

        public Task Publish<T>(IList<T> events, CancellationToken cancellationToken = default) where T : IEvent
        {
            foreach (var @event in events)
            {
                Publish(@event, cancellationToken);
            }

            return Task.CompletedTask;
        }

        private void OnConsumerConsumerCancelled(object sender, ConsumerEventArgs e)
        {
            _log.Warning("Consumer {consumerTag} cancelled", (sender as EventingBasicConsumer).ConsumerTag);
        }

        private void OnConsumerUnregistered(object sender, ConsumerEventArgs e)
        {
            _log.Warning("Consumer {consumerTag} unregistered", (sender as EventingBasicConsumer).ConsumerTag);
        }

        private void OnConsumerRegistered(object sender, ConsumerEventArgs e)
        {
            _log.Debug("Consumer {consumerTag} is registered", (sender as EventingBasicConsumer).ConsumerTag);
        }

        private void OnConsumerShutdown(object sender, ShutdownEventArgs e)
        {
            _log.Warning("Consumer {consumerTag} shut down", (sender as EventingBasicConsumer).ConsumerTag);
        }

        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e) { }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            _log.Verbose("Checking Rabbit Event Service health status");

            return Task.FromResult(!_connection.IsOpen
                ? HealthCheckResult.Unhealthy("Connection to RabbitMQ is lost")
                : !_consumers.Any(consumer => consumer.IsRunning)
                  ? HealthCheckResult.Unhealthy("No consumers are running")
                  :_consumers.Any(consumer => !consumer.IsRunning)
                    ? HealthCheckResult.Degraded("One or more consumers are not running")
                    : HealthCheckResult.Healthy("Rabbit Event Service is working properly"));

        }
    }
}