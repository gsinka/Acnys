using System;
using System.Collections.Generic;
using RabbitMQ.Client;

namespace Acnys.Core.Hosting.RabbitMQ
{
    public class RabbitConnectionFactory
    {
        private readonly IList<Action<IConnection>> _buildCallbacks = new List<Action<IConnection>>();
        private readonly IList<Action<ConnectionFactory>> _configurationCallbacks = new List<Action<ConnectionFactory>>();

        public void RegisterBuildCallback(Action<IConnection> callback)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));
            _buildCallbacks.Add(callback);
        }

        public void RegisterConfigurationCallback(Action<ConnectionFactory> callback)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));
            _configurationCallbacks.Add(callback);
        }

        public IConnection CreateConnection()
        {
            var factory = new ConnectionFactory();

            foreach (var action in _configurationCallbacks)
            {
                action(factory);
            }

            var connection = factory.CreateConnection();

            foreach (var action in _buildCallbacks)
            {
                action(connection);
            }

            return connection;
        }
    }
}