using System.Collections.Generic;
using System.Net;
using Acnys.Core.Eventing.Abstractions;
using Autofac;
using RabbitMQ.Client;
using Serilog;

namespace Acnys.Core.RabbitMQ
{
    public class RabbitService
    {
        private readonly ILogger _log;
        
        private readonly IDispatchEvent _eventDispatcher;
        private readonly List<EventListener> _listeners = new List<EventListener>();
            
        public readonly IConnection Connection;
        public IModel Model { get; }

        public RabbitService(ILogger log, IConnection connection, IDispatchEvent eventDispatcher)
        {
            _log = log;
            Connection = connection;
            _eventDispatcher = eventDispatcher;
            Model = connection.CreateModel();
        }

        public void CreateExchange(string name, string type = ExchangeType.Fanout, bool durable = false, bool autoDelete = false, IDictionary<string, object> arguments = null)
        {
            Model.ExchangeDeclare(name, type, durable, autoDelete, arguments);
        }

        public void CreateQueue(string name, bool durable = false, bool exclusive = false, bool autoDelete = false, IDictionary<string, object> arguments = null)
        {
            Model.QueueDeclare(name, durable, exclusive, autoDelete, arguments);
        }

        public void Bind(string queue, string exchange, string routingKey = "", IDictionary<string, object> arguments = null)
        {
            Model.QueueBind(queue, exchange, routingKey, arguments);
        }

        public void AddEventListener(string queue, string consumerTag = null, IDictionary<string, object> arguments = null)
        {
            _listeners.Add(new EventListener(_log, Connection, _eventDispatcher, queue, consumerTag ?? "", arguments, EventListener.Default));
        }
    }
}