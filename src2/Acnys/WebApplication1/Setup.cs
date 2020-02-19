using Autofac;
using Acnys.Core.RabbitMQ;

namespace WebApplication1
{
    public class Setup : IStartable
    {
        private readonly IRabbitService _rabbitService;

        public Setup(IRabbitService rabbitService)
        {
            _rabbitService = rabbitService;
        }

        public void Start()
        {
            _rabbitService.CreateQueue("test", autoDelete: true);
            _rabbitService.CreateExchange("test", autoDelete: true);
            _rabbitService.Bind("test", "test");
        }
    }
}
