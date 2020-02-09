using Acnys.Core.Eventing;
using RabbitMQ.Client.Events;

namespace Acnys.Core.Hosting.RabbitMQ
{
    public interface IMapEvent
    {
        IEvent ToEvent(BasicDeliverEventArgs args);
    }
}
