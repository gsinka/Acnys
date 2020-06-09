using System.Collections.Generic;
using RabbitMQ.Client;

namespace Acnys.Core.RabbitMQ
{
    public interface IRabbitService //: IPublishEvent
    {
        void CreateExchange(string name, string type = ExchangeType.Fanout, bool durable = false, bool autoDelete = false, IDictionary<string, object> arguments = null);
        void CreateQueue(string name, bool durable = false, bool exclusive = false, bool autoDelete = false, IDictionary<string, object> arguments = null);
        void Bind(string queue, string exchange, string routingKey = "", IDictionary<string, object> arguments = null);
    }
}