using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Acnys.Core.Eventing.Abstractions;
using Acnys.Core.Extensions;
using Acnys.Core.RabbitMQ.Extensions;
using Acnys.Core.ValueObjects;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;
using Serilog.Context;

namespace Acnys.Core.RabbitMQ
{
    public class RabbitEventListener
    {
        private readonly ILogger _log;
        private readonly IConnection _connection;
        private readonly IDispatchEvent _eventDispatcher;
        public readonly string Queue;
        public readonly string ConsumerTag;
        private readonly IDictionary<string, object> _consumerArgs;
        private readonly Func<ILogger, EventingBasicConsumer, BasicDeliverEventArgs, (IEvent evnt, IDictionary<string, object> args)> _eventMapper;
        public EventingBasicConsumer Consumer;

        public RabbitEventListener(
            ILogger log,
            IConnection connection,
            IDispatchEvent eventDispatcher,
            string queue,
            string consumerTag,
            IDictionary<string, object> consumerArgs,
            Func<ILogger, EventingBasicConsumer, BasicDeliverEventArgs, (IEvent evnt, IDictionary<string, object> args)> eventMapper)
        {
            _log = log;
            _connection = connection;
            _eventDispatcher = eventDispatcher;
            Queue = queue;
            ConsumerTag = consumerTag;
            _consumerArgs = consumerArgs;
            _eventMapper = eventMapper;
        }

        public void Start()
        {
            var channel = _connection.CreateModel();

            Consumer = new EventingBasicConsumer(channel);

            Consumer.Received += OnReceived;
            Consumer.Shutdown += OnConsumerShutdown;
            Consumer.Registered += OnConsumerRegistered;
            Consumer.Unregistered += OnConsumerUnregistered;
            Consumer.ConsumerCancelled += OnConsumerConsumerCancelled;

            try
            {
                var tagReceived = channel.BasicConsume(Queue, false, ConsumerTag, _consumerArgs, Consumer);
                _log.Debug("RabbitMQ consumer with tag '{consumerTag}' created for event listener", tagReceived);
            }
            catch (Exception exception)
            {
                _log.Error(exception, "RabbitMQ consumer failed to create for {queue}", Queue);
            }
        }

        private void OnReceived(object sender, BasicDeliverEventArgs e)
        {
            try
            {
                _log.Debug("Receiving new message from exchange '{exchange}' with routing key '{routingKey}'", e.Exchange, e.RoutingKey);

                var (evnt, args) = _eventMapper(_log, Consumer, e);
                args.EnrichLogContextWithCorrelation();

                _eventDispatcher.Dispatch(evnt, args, CancellationToken.None);

                _log.Debug("Sending ACK to message queue for delivery tag '{deliveryTag}'", e.DeliveryTag);
                Consumer.Model.BasicAck(e.DeliveryTag, false);

            }
            catch (Exception exception)
            {
                _log.Error(exception, "Message delivery failed");
                Consumer.Model.BasicNack(e.DeliveryTag, false, false);
            }
        }

        public static (IEvent evnt, IDictionary<string, object> args) Default(ILogger log, EventingBasicConsumer consumer, BasicDeliverEventArgs args)
        {
            IEvent evnt;

            try
            {
                var eventJson = Encoding.UTF8.GetString(args.Body);

                if (args.BasicProperties.IsTypePresent())
                {
                    // Type in properties

                    var eventType = Type.GetType(args.BasicProperties.Type);

                    if (!typeof(IEvent).IsAssignableFrom(eventType))
                    {
                        log.Error("Message is not an event");
                        return (null, null);
                    }

                    evnt = (IEvent)JsonConvert.DeserializeObject(eventJson, eventType, new JsonSerializerSettings() { TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple });
                }
                else
                {
                    // Try to get type from json ($type)

                    evnt = (IEvent)JsonConvert.DeserializeObject(eventJson, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto, TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple });
                }

            }
            catch (Exception exception)
            {
                throw new InvalidOperationException("Cannot deserialize message to event. Either add type property to message header or put type information into the JSON.", exception);
            }

            var eventArgs = (args.BasicProperties?.Headers ?? new Dictionary<string, object>())
                .Where(pair => pair.Key != RequestConstants.CausationId && pair.Key != nameof(args.RoutingKey))
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            if (args.BasicProperties.IsCorrelationIdPresent())
            {
                if (Guid.TryParse(args.BasicProperties.CorrelationId, out var result))
                    eventArgs.UseCorrelationId(result);
            }

            if (args.BasicProperties?.Headers?.ContainsKey(RequestConstants.CausationId) ?? false)
            {
                var causationId = Encoding.UTF8.GetString((byte[])args.BasicProperties.Headers[RequestConstants.CausationId]);
                if (Guid.TryParse(causationId.ToString(), out var result)) eventArgs.UseCausationId(result);
            }

            eventArgs.Add(nameof(args.DeliveryTag), args.DeliveryTag);
            eventArgs.Add(nameof(args.ConsumerTag), args.ConsumerTag);
            eventArgs.Add(nameof(args.Exchange), args.Exchange);
            eventArgs.Add(nameof(args.Redelivered), args.Redelivered);
            if (!eventArgs.ContainsKey(nameof(args.RoutingKey))) eventArgs.Add(nameof(args.RoutingKey), args.RoutingKey);

            return (evnt, eventArgs);
        }

        private void OnConsumerConsumerCancelled(object sender, ConsumerEventArgs e)
        {
            _log.Error("Consumer {consumerTag} cancelled", (sender as EventingBasicConsumer).ConsumerTag);
        }

        private void OnConsumerUnregistered(object sender, ConsumerEventArgs e)
        {
            _log.Error("Consumer {consumerTag} unregistered", (sender as EventingBasicConsumer).ConsumerTag);
        }

        private void OnConsumerRegistered(object sender, ConsumerEventArgs e)
        {
            _log.Debug("Consumer {consumerTag} is registered", (sender as EventingBasicConsumer).ConsumerTag);
        }

        private void OnConsumerShutdown(object sender, ShutdownEventArgs e)
        {
            _log.Warning("Consumer {consumerTag} shut down", (sender as EventingBasicConsumer).ConsumerTag);
        }

    }
}