using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Acnys.Core.Eventing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;
using Serilog;

namespace Acnys.Core.Hosting.RabbitMQ
{
    /// <summary>
    /// Basic mapper
    /// </summary>
    /// <remarks>
    /// This mapper will map event's eventId, correlationId and causationId as message properties and vice versa
    /// </remarks>
    public class BasicPropertiesMapper : IMapEvent
    {
        private readonly ILogger _log;

        public BasicPropertiesMapper(ILogger log)
        {
            _log = log;
        }

        public IEvent ToEvent(BasicDeliverEventArgs args)
        {
            _log.Debug("Mapping RabbitMQ event to event object");

            var json = Encoding.UTF8.GetString(args.Body);

            _log.Verbose("BasicDeliverEventArgs body: {eventArgs}", json);

            _log.Debug("Parsing event with EventId and CorrelationId");

            try
            {
                var orig = JObject.Parse(json);
                orig["EventId"] = args.BasicProperties.MessageId ?? Guid.NewGuid().ToString();
                orig["CorrelationId"] = args.BasicProperties.CorrelationId ?? string.Empty;

                if (args.BasicProperties.IsHeadersPresent() && args.BasicProperties.Headers.ContainsKey(nameof(ICausedBy.CausationId))) 
                    orig[nameof(ICausedBy.CausationId)] = Encoding.UTF8.GetString((byte[])args.BasicProperties.Headers[nameof(ICausedBy.CausationId)]);

                var full = orig.ToString();

                var type = Type.GetType(args.BasicProperties.Type);
                _log.Debug("Type for event defined as {eventType}", type.FullName);

                var evnt = JsonConvert.DeserializeObject(full, type);
                _log.Debug("Event deserialized");

                return (IEvent)evnt;

            }
            catch (Exception exception)
            {
                _log.Error(exception, "Event deserialization failed.");
                return null;
            }
        }

        public (BasicProperties props, byte[] body) FromEvent<T>(T @event, IDictionary<string, object> properties = null) where T : IEvent
        {
            _log.Debug("Mapping event object to RabbitMQ event");

            if (properties == null)
            {
                properties = new Dictionary<string, object>()
                {
                    { nameof(ICausedBy.CausationId), ((ICausedBy)@event)?.CausationId.ToString() }
                };
            }
            else if (!properties.ContainsKey(nameof(ICausedBy.CausationId)))
            {
                properties.Add(nameof(ICausedBy.CausationId), ((ICausedBy)@event)?.CausationId.ToString());
            }

            var props = new BasicProperties()
            {
                MessageId = @event.EventId.ToString(),
                CorrelationId = ((ICorrelatedBy)@event)?.CorrelationId.ToString(),
                Type = @event.GetType().AssemblyQualifiedName,
                ContentEncoding = "UTF8",
                ContentType = "text/json",
                Headers = properties,
            };

            _log.Verbose("Basic properties constructed: {@props}", props);

            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event, new JsonSerializerSettings() { ContractResolver = new EventResolver() }));
            _log.Debug("Event data constructed");

            return (props, body);
        }

        /// <summary>
        /// Event resolver to remove eventId, causationId and correlationId from the json
        /// </summary>
        internal class EventResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);

                if (property.PropertyName == "EventId" || property.PropertyName == "CausationId" || property.PropertyName == "CorrelationId")
                {
                    property.Ignored = true;
                }

                return property;
            }
        }
    }
}