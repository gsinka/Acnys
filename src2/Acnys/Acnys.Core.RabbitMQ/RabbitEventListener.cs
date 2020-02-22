using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Acnys.Core.Abstractions.Extensions;
using Acnys.Core.Eventing.Abstractions;
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
                var (evnt, args) = _eventMapper(_log, Consumer, e);

                using var correlationId = LogContext.PushProperty("correlationId", args.CorrelationId());
                using var causationId = LogContext.PushProperty("causationId", args.CausationId());

                _eventDispatcher.Dispatch(evnt, args, CancellationToken.None);
                Consumer.Model.BasicAck(e.DeliveryTag, false);

            }
            catch (Exception exception)
            {
                _log.Error(exception, "Message delivery failed");
                Consumer.Model.BasicNack(e.DeliveryTag, false, false);
            }
        }

        public static (IEvent evnt, IDictionary<string, object> args) Default(ILogger log,  EventingBasicConsumer consumer, BasicDeliverEventArgs args)
        {
            if (args.BasicProperties.Type == null)
            {
                log.Error("Missing event type");
                return (null, null);
            }

            var eventType = Type.GetType(args.BasicProperties.Type);

            if (!typeof(IEvent).IsAssignableFrom(eventType))
            {
                log.Error("Message is not an event");
                return (null, null);
            }
            
            var eventJson = Encoding.UTF8.GetString(args.Body);
            var evnt = (IEvent)JsonConvert.DeserializeObject(eventJson, eventType);

            var eventArgs = args.BasicProperties.Headers.Where(pair => pair.Key != CorrelationExtensions.CausationIdName).ToDictionary(pair => pair.Key, pair => pair.Value);

            if (args.BasicProperties.IsCorrelationIdPresent())
            {
                if (Guid.TryParse(args.BasicProperties.CorrelationId, out var result))
                    eventArgs.UseCorrelationId(result);
            }

            if (args.BasicProperties.Headers.ContainsKey(CorrelationExtensions.CausationIdName))
            {
                var causationId = Encoding.UTF8.GetString((byte[])args.BasicProperties.Headers[CorrelationExtensions.CausationIdName]);
                
                if (Guid.TryParse(causationId.ToString(), out var result))
                    eventArgs.UseCausationId(result);
            }

            eventArgs.Add(nameof(args.DeliveryTag), args.DeliveryTag);
            eventArgs.Add(nameof(args.ConsumerTag), args.ConsumerTag);
            eventArgs.Add(nameof(args.Exchange), args.Exchange);
            eventArgs.Add(nameof(args.Redelivered), args.Redelivered);
            eventArgs.Add(nameof(args.RoutingKey), args.RoutingKey);

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