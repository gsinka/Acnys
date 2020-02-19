﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Eventing.Abstractions;
using RabbitMQ.Client;
using Serilog;

namespace Acnys.Core.RabbitMQ
{
    public class RabbitService : IRabbitService
    {
        private readonly ILogger _log;
        
        private readonly IDispatchEvent _eventDispatcher;
        private readonly List<EventListener> _listeners = new List<EventListener>();
            
        public readonly IConnection Connection;
        private readonly EventPublisher _publisher;
        public IModel Model { get; }

        public RabbitService(ILogger log, IConnection connection, IDispatchEvent eventDispatcher, string publisherExchange)
        {
            _log = log;
            Connection = connection;
            _eventDispatcher = eventDispatcher;
            Model = connection.CreateModel();

            connection.ConnectionShutdown += (sender, args) => _log.Error("RabbitMQ connection {endpoint} closed", connection.Endpoint.ToString());
            
            _log.Debug("Creating event publisher on exchange {publisherExchange}", publisherExchange);
            _publisher = new EventPublisher(_log.ForContext<EventPublisher>(), Connection, publisherExchange , EventPublisher.DefaultContextBuilder);

        }

        public void CreateExchange(string name, string type = ExchangeType.Fanout, bool durable = false, bool autoDelete = false, IDictionary<string, object> arguments = null)
        {
            _log.Verbose("Creating RabbitMQ exchange '{name}'. Type: {type}, durable: {durable}, auto-delete: {autoDelete}", name, type, durable, autoDelete);
            Model.ExchangeDeclare(name, type, durable, autoDelete, arguments);
        }

        public void CreateQueue(string name, bool durable = false, bool exclusive = false, bool autoDelete = false, IDictionary<string, object> arguments = null)
        {
            _log.Verbose("Creating RabbitMQ queue '{name}'. Durable: {durable}, exclusive: {exclusive}, auto-delete: {autoDelete}", name, durable, exclusive, autoDelete);
            Model.QueueDeclare(name, durable, exclusive, autoDelete, arguments);
        }

        public void Bind(string queue, string exchange, string routingKey = "", IDictionary<string, object> arguments = null)
        {
            _log.Debug("Binding queue {queue} with exchange {exchange} with routing key '{routingKey}'", queue, exchange, routingKey);
            Model.QueueBind(queue, exchange, routingKey, arguments);
        }

        public void AddEventListener(string queue, string consumerTag = null, IDictionary<string, object> arguments = null)
        {
            _listeners.Add(new EventListener(_log, Connection, _eventDispatcher, queue, consumerTag ?? "", arguments, EventListener.Default));
        }

        public async Task Publish<T>(T @event, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default) where T : IEvent
        {
            await _publisher.Publish(@event, arguments, cancellationToken);
        }
    }
}