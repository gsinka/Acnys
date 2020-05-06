using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Acnys.Core.Eventing.Abstractions;
using Acnys.Core.Extensions;
using Acnys.Core.ValueObjects;
using Autofac;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;

namespace Acnys.Core.RabbitMQ
{
    public class RabbitEventListener
    {
        private readonly ILogger _log;
        private readonly IConnection _connection;
        private readonly ILifetimeScope _scope;
        public readonly string Queue;
        public readonly string ConsumerTag;
        private readonly int _consumerCount;
        private readonly bool _requeueOnNack;
        private readonly IDictionary<string, object> _consumerArgs;
        private readonly Func<ILogger, EventingBasicConsumer, BasicDeliverEventArgs, (IEvent evnt, IDictionary<string, object> args)> _eventMapper;
        //public EventingBasicConsumer Consumer;

        public IList<EventingBasicConsumer> _consumers = new List<EventingBasicConsumer>();

        public RabbitEventListener(
            ILogger log,
            IConnection connection,
            ILifetimeScope scope,
            string queue,
            string consumerTag,
            int consumerCount,
            bool requeueOnNack,
            IDictionary<string, object> consumerArgs,
            Func<ILogger, EventingBasicConsumer, BasicDeliverEventArgs, (IEvent evnt, IDictionary<string, object> args)> eventMapper)
        {
            _log = log;
            _connection = connection;
            _scope = scope;
            Queue = queue;
            ConsumerTag = consumerTag;
            _consumerCount = consumerCount;
            _requeueOnNack = requeueOnNack;
            _consumerArgs = consumerArgs;
            _eventMapper = eventMapper;
        }

        public void Start()
        {
            var channel = _connection.CreateModel();
            
            for (var i = 0; i < _consumerCount; i++)
            {
                var consumer = new EventingBasicConsumer(channel);

                consumer.Received += OnReceived;
                consumer.Shutdown += OnConsumerShutdown;
                consumer.Registered += OnConsumerRegistered;
                consumer.Unregistered += OnConsumerUnregistered;
                consumer.ConsumerCancelled += OnConsumerConsumerCancelled;

                try
                {
                    var tag = !string.IsNullOrWhiteSpace(ConsumerTag)
                        ? _consumerCount > 1 ? $"{ConsumerTag}-{i + 1}" : ConsumerTag
                        : "";

                    var tagReceived = channel.BasicConsume(Queue, false, tag, _consumerArgs, consumer);
                    _log.Debug("RabbitMQ consumer with tag '{consumerTag}' created for event listener", tagReceived);
                }
                catch (Exception exception)
                {
                    _log.Error(exception, "RabbitMQ consumer failed to create for {queue}", Queue);
                }
            }
        }

        private void OnReceived(object sender, BasicDeliverEventArgs e)
        {
            using var scope = _scope.BeginLifetimeScope();
            var consumer = sender as EventingBasicConsumer;

            _log.Debug("New lifetime scope created for event dispatcher ({scopeId})", scope.GetHashCode());

            try
            {
                _log.Debug("Receiving new message from exchange '{exchange}' with routing key '{routingKey}' by consumer '{consumerTag}'",
                    e.Exchange, e.RoutingKey, consumer.ConsumerTag);

                var (evnt, args) = _eventMapper(_log, consumer, e);
                args.EnrichLogContextWithCorrelation();

                var dispatchTask = scope.Resolve<IDispatchEvent>().Dispatch(evnt, args, CancellationToken.None);
                dispatchTask.Wait();

                _log.Debug("Sending ACK to message queue for delivery tag '{deliveryTag}'", e.DeliveryTag);
                consumer.Model.BasicAck(e.DeliveryTag, false);
            }
            catch (Exception exception)
            {
                _log.Error(exception, "Message delivery failed");
                consumer.Model.BasicNack(e.DeliveryTag, false, _requeueOnNack);
            }
            finally
            {
                _log.Debug("Ending lifetime scope ({scopeId})", scope.GetHashCode());

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