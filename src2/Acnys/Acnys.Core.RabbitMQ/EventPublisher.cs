using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Abstractions;
using Acnys.Core.Eventing.Abstractions;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;

namespace Acnys.Core.RabbitMQ
{
    public class EventPublisher : IPublishEvent
    {
        private readonly IConnection _connection;
        private readonly string _exchange;
        private readonly Func<IEvent, IDictionary<string, object>, (string routingKey, bool mandatory, IBasicProperties properties, byte[] body)> _routingKeySelector;

        public EventPublisher(IConnection connection, string exchange, Func<IEvent, IDictionary<string, object>, (string routingKey, bool mandatory, IBasicProperties properties, byte[] body)> routingKeySelector)
        {
            _connection = connection;
            _exchange = exchange;
            _routingKeySelector = routingKeySelector;
        }

        public Task Publish<T>(T @event, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default) where T : IEvent
        {
            var (routingKey, mandatory, props, body) = _routingKeySelector(@event, arguments);
            var channel = _connection.CreateModel();
            channel.BasicPublish(_exchange, routingKey, mandatory, props, body);

            return Task.CompletedTask;
        }

        public static (string routingKey, bool mandatory, IBasicProperties properties, byte[] body) Default(IEvent evnt, IDictionary<string, object> args)
        {
            var basicProps = new BasicProperties()
            {
                ContentType = "application/json",
                Type = evnt.GetType().AssemblyQualifiedName,
                Headers = new Dictionary<string, object>(),
            };

            if (args != null)
            { 
                foreach (var arg in args)
                {
                    basicProps.Headers.Add(arg);
                }
            }

            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(evnt));
            
            return (routingKey: "", mandatory: false, properties: basicProps, body);
        }

    }
}
