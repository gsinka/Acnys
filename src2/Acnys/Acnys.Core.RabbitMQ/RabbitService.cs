using System.Collections.Generic;
using RabbitMQ.Client;
using Serilog;

namespace Acnys.Core.RabbitMQ
{
    public class RabbitService : IRabbitService
    {
        private readonly ILogger _log;
        public readonly IConnection Connection;
        public IModel Model { get; }

        public RabbitService(ILogger log, IConnection connection)
        {
            _log = log;
            Connection = connection;
            Model = connection.CreateModel();

            connection.ConnectionShutdown += (sender, args) => _log.Error("RabbitMQ connection {endpoint} closed", connection.Endpoint.ToString());
        }

        public void CreateExchange(string name, string type = ExchangeType.Fanout, bool durable = false, bool autoDelete = false, IDictionary<string, object> arguments = null)
        {
            _log.Debug("Creating RabbitMQ exchange '{name}'. Type: {type}, durable: {durable}, auto-delete: {autoDelete}", name, type, durable, autoDelete);
            Model.ExchangeDeclare(name, type, durable, autoDelete, arguments);
        }

        public void CreateQueue(string name, bool durable = false, bool exclusive = false, bool autoDelete = false, IDictionary<string, object> arguments = null)
        {
            _log.Debug("Creating RabbitMQ queue '{name}'. Durable: {durable}, exclusive: {exclusive}, auto-delete: {autoDelete}", name, durable, exclusive, autoDelete);
            Model.QueueDeclare(name, durable, exclusive, autoDelete, arguments);
        }

        public void Bind(string queue, string exchange, string routingKey = "", IDictionary<string, object> arguments = null)
        {
            _log.Debug("Binding queue {queue} with exchange {exchange} with routing key '{routingKey}'", queue, exchange, routingKey);
            Model.QueueBind(queue, exchange, routingKey, arguments);
        }
    }
}