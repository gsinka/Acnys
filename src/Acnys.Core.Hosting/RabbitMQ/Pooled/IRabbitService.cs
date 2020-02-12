using Acnys.Core.Request.Application;
using RabbitMQ.Client;

namespace Acnys.Core.Hosting.RabbitMQ.Pooled
{
    public interface IRabbitService : IPublishEvent
    {
        IModel Channel { get; }
    }
}