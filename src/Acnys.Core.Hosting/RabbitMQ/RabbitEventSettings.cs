using System.Collections.Generic;
using RabbitMQ.Client;

namespace Acnys.Core.Hosting.RabbitMQ
{
    public class RabbitEventSettings
    {
        public string Uri { get; set; }

        public RabbitEventQueueSettings Queue { get; set; }

        public RabbitEventExchangeSettings Exchange { get; set; }

        public string RoutingKey { get; set; } = "";
    }

    public class RabbitEventExchangeSettings
    {
        public string Name { get; set; }
        public string Type { get; set; } = ExchangeType.Topic;
        public IDictionary<string, string> Bindings { get; set; } = new Dictionary<string, string>();
    }

    public class RabbitEventQueueSettings
    {
        public string Name { get; set; }
        public bool AutoDelete { get; set; } = true;
        public bool Durable { get; set; } = false;
        public bool Exclusive { get; set; } = false;
        public IDictionary<string, object> Arguments { get; set; }
        public IDictionary<string, string> Bindings { get; set; } = new Dictionary<string, string>();
    }
}