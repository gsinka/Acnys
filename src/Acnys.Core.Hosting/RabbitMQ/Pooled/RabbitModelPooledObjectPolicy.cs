using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Client;

namespace Acnys.Core.Hosting.RabbitMQ.Pooled
{
    public class RabbitModelPooledObjectPolicy : IPooledObjectPolicy<IModel>
    {
        private readonly IConnection _connection;

        public RabbitModelPooledObjectPolicy(IConnection connection)
        {
            _connection = connection;
        }

        public IModel Create()
        {
            return _connection.CreateModel();
        }

        public bool Return(IModel obj)
        {
            if (obj.IsOpen) return true;

            obj?.Dispose();
            return false;
        }
    }
}
