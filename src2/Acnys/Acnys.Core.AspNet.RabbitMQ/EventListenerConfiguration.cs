using System.Collections.Generic;


namespace Acnys.Core.AspNet.RabbitMQ
{
    public class EventListenerConfiguration
    {
        public string Queue { get; set; }
        public string ConsumerTag { get; set; } = "";
        public Dictionary<string, object> ConsumerArguments { get; set; } = new Dictionary<string, object>();
    }
}
