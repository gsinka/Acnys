using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Eventing.Abstractions;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;

namespace Acnys.Core.RabbitMQ
{
    /// <summary>
    /// RabbitMQ event publisher
    /// </summary>
    public class EventPublisher : IPublishEvent
    {
        public static string RoutingKey = nameof(RoutingKey);
        public static string Mandatory = nameof(Mandatory);

        private readonly string _exchange;
        private readonly IModel _model;
        private readonly Func<IEvent, IDictionary<string, object>, (string routingKey, bool mandatory, IBasicProperties properties, byte[] body)> _publishContext;

        /// <summary>
        /// Creates a new EventPublisher service
        /// </summary>
        /// <param name="connection">RabbitMQ connection instance</param>
        /// <param name="exchange">Exchange to use for event publishing</param>
        /// <param name="publishContext">Publish context builder function for building routing key, mandatory flag and properties</param>
        public EventPublisher(
            IConnection connection, 
            string exchange, 
            Func<IEvent, IDictionary<string, object>, (string routingKey, bool mandatory, IBasicProperties properties, byte[] body)> publishContext)
        {
            _model = connection.CreateModel();
            _exchange = exchange;
            _publishContext = publishContext;
        }

        public Task Publish<T>(T @event, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default) where T : IEvent
        {
            var (routingKey, mandatory, props, body) = _publishContext(@event, arguments);
            _model.BasicPublish(_exchange, routingKey, mandatory, props, body);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Default publish context builder function
        /// </summary>
        /// <param name="evnt">Event</param>
        /// <param name="args">Arguments</param>
        /// <returns>Routing key, mandatory flag and basic properties</returns>
        /// <remarks>
        /// This function tries to extract routing key from the arguments. If key exists with 'RoutingKey' the value will be used.
        /// </remarks>
        public static (string routingKey, bool mandatory, IBasicProperties properties, byte[] body) DefaultContextBuilder(IEvent evnt, IDictionary<string, object> args)
        {
            if (evnt == null) return (string.Empty, false, null, null);

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
            
            return (
                routingKey: (args != null && args.ContainsKey(RoutingKey)) ? args[RoutingKey].ToString() : string.Empty, 
                mandatory: (args != null && args.ContainsKey(Mandatory) && args[Mandatory] is bool) && ((bool)args[Mandatory]), 
                properties: basicProps, body);
        }
    }
}
