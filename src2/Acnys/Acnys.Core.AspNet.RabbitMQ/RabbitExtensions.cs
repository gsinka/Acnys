using Autofac;
using Microsoft.Extensions.Hosting;

namespace Acnys.Core.AspNet.RabbitMQ
{
    public static class RabbitExtensions
    {
        public static IHostBuilder AddRabbitService(this IHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureContainer<ContainerBuilder>((context, builder) =>
            {
                builder.RegisterType<RabbitHostedService>().AsImplementedInterfaces().SingleInstance();
            });
        }
    }
}
