namespace Acnys.Core.Hosting.RabbitMQ
{
    public class RabbitEventSettings
    {
        public string Uri { get; set; }
        public string EventQueue { get; set; }
        public string EventExchange { get; set; }
        public string RoutingKey { get; set; }
    }
}