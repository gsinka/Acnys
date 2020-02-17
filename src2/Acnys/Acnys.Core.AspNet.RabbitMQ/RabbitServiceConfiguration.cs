using System.Collections.Generic;

namespace Acnys.Core.AspNet.RabbitMQ
{
    public class RabbitServiceConfiguration
    {
        public string Uri { get; set; }

        public EventPublisherConfiguration Publisher { get; set; }

        public List<EventListenerConfiguration> Listeners { get; set; }

    }
}