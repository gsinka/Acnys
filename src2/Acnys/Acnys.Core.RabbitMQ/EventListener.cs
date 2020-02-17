using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Acnys.Core.Eventing.Abstractions;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;

namespace Acnys.Core.RabbitMQ
{
    public class EventListener
    {
        private readonly ILogger _log;
        private readonly IDispatchEvent _eventDispatcher;
        private readonly Func<ILogger, EventingBasicConsumer, BasicDeliverEventArgs, (IEvent evnt, IDictionary<string, object> args)> _eventMapper;
        private readonly EventingBasicConsumer _consumer;

        public EventListener(
            ILogger log,
            IConnection connection, 
            IDispatchEvent eventDispatcher,
            string queue,
            string consumerTag,
            IDictionary<string, object> consumerArgs,
            Func<ILogger, EventingBasicConsumer, BasicDeliverEventArgs, (IEvent evnt, IDictionary<string, object> args)> eventMapper)
        {
            _log = log;
            _eventDispatcher = eventDispatcher;
            _eventMapper = eventMapper;

            var channel = connection.CreateModel();
            
            _consumer = new EventingBasicConsumer(channel);
            _consumer.Received += OnReceived;

            channel.BasicConsume(queue, false, consumerTag, consumerArgs, _consumer);
        }

        private void OnReceived(object sender, BasicDeliverEventArgs e)
        {
            try
            {
                var (evnt, args) = _eventMapper(_log, _consumer, e);
                _eventDispatcher.Dispatch(evnt, args, CancellationToken.None);
                _consumer.Model.BasicAck(e.DeliveryTag, false);

            }
            catch (Exception exception)
            {
                _log.Error(exception, "Message delivery failed");
                _consumer.Model.BasicNack(e.DeliveryTag, false, false);
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

            var eventArgs = args.BasicProperties.Headers.ToDictionary(pair => pair.Key, pair => pair.Value);
            
            eventArgs.Add(nameof(args.DeliveryTag), args.DeliveryTag);
            eventArgs.Add(nameof(args.ConsumerTag), args.ConsumerTag);
            eventArgs.Add(nameof(args.Exchange), args.Exchange);
            eventArgs.Add(nameof(args.Redelivered), args.Redelivered);
            eventArgs.Add(nameof(args.RoutingKey), args.RoutingKey);

            return (evnt, eventArgs);
        }
    }
}