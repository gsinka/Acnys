using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Eventing;
using Acnys.Core.Request.Application;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;
using Serilog;

namespace Acnys.Core.Hosting.RabbitMQ.Lovisa
{
    public class RabbitEventService : BackgroundService, IRabbitEventService, IPublishEvent
    {
        private readonly ILogger _log;
        private readonly IDispatchEvent _eventDispatcher;
        private readonly IMapEvent _eventMapper;
        private readonly IOptions<RabbitEventSettings> _settings;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitEventService(ILogger log, IDispatchEvent eventDispatcher, IOptions<RabbitEventSettings> settings)
        {
            _log = log;
            _eventDispatcher = eventDispatcher;
            _eventMapper = new BasicPropertiesMapper(log);
            _settings = settings;

            var factory = new ConnectionFactory() { Uri = new Uri(_settings.Value.Uri) };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            _channel.ExchangeDeclare(_settings.Value.EventExchange, ExchangeType.Topic);
            _channel.QueueDeclare(_settings.Value.EventQueue, true, false, false, null);
            _channel.QueueBind(_settings.Value.EventQueue, _settings.Value.EventExchange, _settings.Value.RoutingKey);
            _channel.BasicQos(0, 1, false);

            for (var core = 0; core < Environment.ProcessorCount; core++)
            {
                var consumer = new EventingBasicConsumer(_channel);
                
                consumer.Received += (sender, args) =>
                {
                    var c = sender as EventingBasicConsumer;
                    _log.Information($"Message received from {c.ConsumerTag}");
                    
                    var evnt = _eventMapper.ToEvent(args);
                    _eventDispatcher.Dispatch(evnt, stoppingToken);
                    c.Model.BasicAck(args.DeliveryTag, false);
                };

                consumer.Shutdown += OnConsumerShutdown;
                consumer.Registered += OnConsumerRegistered;
                consumer.Unregistered += OnConsumerUnregistered;
                consumer.ConsumerCancelled += OnConsumerConsumerCancelled;

                _channel.BasicConsume(_settings.Value.EventQueue, false, consumer);
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
            var mapper = new BasicPropertiesMapper(_log);
            var (props, body) = mapper.FromEvent(evnt, new Dictionary<string, object>());
            _channel.BasicPublish(_settings.Value.EventExchange, "", false, props, body);

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

        private void OnConsumerConsumerCancelled(object sender, ConsumerEventArgs e) { }
        private void OnConsumerUnregistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerRegistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerShutdown(object sender, ShutdownEventArgs e) { }
        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e) { }

    }
}