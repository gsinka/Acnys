using System.Collections.Generic;
using System.Net.WebSockets;
using Autofac;
using Acnys.Core.RabbitMQ;

namespace WebApplication1
{
    public class Setup : IStartable
    {
        private readonly IRabbitService _rabbitService;
        private readonly IEnumerable<RabbitEventListener> _listeners;

        public Setup(IRabbitService rabbitService, IEnumerable<RabbitEventListener> listeners)
        {
            _rabbitService = rabbitService;
            _listeners = listeners;
        }

        public void Start()
        {
            _rabbitService.CreateQueue("test", autoDelete: true);
            _rabbitService.CreateExchange("test", autoDelete: true);
            _rabbitService.Bind("test", "test");
        }
    }
}
