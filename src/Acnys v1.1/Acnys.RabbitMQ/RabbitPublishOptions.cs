namespace Acnys.Request.Rabbit
{
    public class RabbitPublishOptions
    {
        public string Exchange { get; }
        public string RoutingKey { get; }
        public bool Mandatory { get; }

        public RabbitPublishOptions(string exchange, string routingKey, bool mandatory)
        {
            Exchange = exchange;
            RoutingKey = routingKey;
            Mandatory = mandatory;
        }
    }
}