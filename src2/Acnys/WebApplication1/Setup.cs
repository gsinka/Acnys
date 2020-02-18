using Autofac;

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
            
        }
    }
}
