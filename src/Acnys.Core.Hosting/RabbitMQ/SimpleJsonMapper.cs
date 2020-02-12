using System;
using System.Collections.Generic;
using System.Text;
using Acnys.Core.Eventing;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;
using Serilog;

namespace Acnys.Core.Hosting.RabbitMQ
{
    /// <summary>
    /// Simple json mapper
    /// </summary>
    /// <remarks>
    /// This mapper uses serializer's type name handling
    /// </remarks>
    public class SimpleJsonMapper : IMapEvent
    {
        private readonly ILogger _log;
        private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };

        public SimpleJsonMapper(ILogger log)
        {
            _log = log;
        }

        public IEvent ToEvent(BasicDeliverEventArgs args)
        {
            _log.Debug("Deserializing event from RabbitMQ message");

            try
            {
                var json = Encoding.UTF8.GetString(args.Body);
                _log.Verbose("Event json {eventJson}", json);

                var evnt = JsonConvert.DeserializeObject(json, _jsonSerializerSettings);
                _log.Debug("Event {eventType} deserialized", evnt.GetType().FullName);
                return (IEvent)evnt;
            }
            catch (Exception exception)
            {
                _log.Error(exception, "Event deserialization failed");
                return null;
            }
        }

        public (BasicProperties props, byte[] body) FromEvent<T>(T @event, IDictionary<string, object> properties = null) where T : IEvent
        {
            _log.Debug("Constructing message from event {eventType}", @event.GetType().Name);

            var json = JsonConvert.SerializeObject(@event, _jsonSerializerSettings);
            _log.Verbose("Event json: {@eventJson}", json);

            var body = Encoding.UTF8.GetBytes(json);
            var basicProperties = new BasicProperties() { Headers = properties };
            _log.Verbose("Message basic properties: {@basicProperties}", basicProperties);

            return (basicProperties, body);
        }
    }
}