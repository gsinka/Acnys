using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Eventing.Abstractions;
using Acnys.Core.Extensions;
using Acnys.Core.ValueObjects;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;
using Serilog;

namespace Acnys.Core.RabbitMQ
{
    /// <summary>
    /// RabbitMQ event publisher
    /// </summary>
    public class RabbitEventPublisher : IPublishEvent
    {
        public static string RoutingKey = nameof(RoutingKey);
        public static string Mandatory = nameof(Mandatory);

        private readonly ILogger _log;
        private readonly string _exchange;
        private readonly IModel _model;
        private readonly Func<IEvent, IDictionary<string, object>, (string routingKey, bool mandatory, IBasicProperties properties, byte[] body)> _publishContext;

        /// <summary>
        /// Creates a new EventPublisher service
        /// </summary>
        /// <param name="connection">RabbitMQ connection instance</param>
        /// <param name="exchange">Exchange to use for event publishing</param>
        /// <param name="publishContext">Publish context builder function for building routing key, mandatory flag and properties</param>
        public RabbitEventPublisher(
            ILogger log,
            IConnection connection, 
            string exchange, 
            Func<IEvent, IDictionary<string, object>, (string routingKey, bool mandatory, IBasicProperties properties, byte[] body)> publishContext)
        {
            _model = connection.CreateModel();
            _log = log;
            _exchange = exchange;
            _publishContext = publishContext;
        }

        public Task Publish<T>(T @event, string routingKey, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default) where T : IEvent
        {
            arguments.EnrichLogContextWithCorrelation();

            var (contextRoutingKey, mandatory, props, body) = _publishContext(@event, arguments);

            _log.Debug("Publishing event '{eventType}' to exchange '{exchangeName}' with routing key '{routingKey}'", typeof(T).Name, _exchange, routingKey ?? contextRoutingKey);
            _log.Verbose("Event data: {@event}", @event);
            _log.Verbose("Mandatory: '{mandatory}', event arguments: {@eventProps}", mandatory, props);

            _model.BasicPublish(_exchange, routingKey ?? contextRoutingKey, mandatory, props, body);
            return Task.CompletedTask;
        }

        public Task Publish<T>(T @event, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default) where T : IEvent
        {
            return Publish(@event, null, arguments, cancellationToken);
        }

        private static readonly Func<Type, string> TypeNameBuilder = type => $"{type.FullName}, {type.Assembly.GetName().Name}";

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
                Type = TypeNameBuilder(evnt.GetType()),
                Headers = new Dictionary<string, object>(),
            };

            if (args != null)
            { 
                foreach (var arg in args)
                {
                    switch (arg.Key)
                    {
                        case RequestConstants.CorrelationId:
                            basicProps.CorrelationId = arg.Value?.ToString() ?? "";
                            break;

                        case RequestConstants.CausationId:
                            basicProps.Headers.Add(RequestConstants.CausationId, arg.Value?.ToString() ?? "");
                            break;

                        default:
                            basicProps.Headers.Add(arg);
                            break;
                    }
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
