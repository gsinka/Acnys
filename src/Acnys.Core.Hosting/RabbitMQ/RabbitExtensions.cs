using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Acnys.Core.Hosting.RabbitMQ
{
    public static class RabbitExtensions
    {
        public static IHostBuilder AddRabbitEventBus(this IHostBuilder hostBuilder, string configSection)
        {
            return hostBuilder

                .ConfigureServices((context, services) =>
                {
                    services.Configure<RabbitEventSettings>(context.Configuration.GetSection(configSection));
                })

                .ConfigureContainer<ContainerBuilder>((context, builder) =>
                {
                    builder.RegisterType<RabbitEventService>().AsSelf().AsImplementedInterfaces().SingleInstance();
                });
        }

        public static IHostBuilder AddRabbitEventBusHealthCheck(this IHostBuilder hostBuilder)
        {
            return hostBuilder

                .ConfigureServices((context, services) =>
                {
                    services.AddHealthChecks().AddCheck<RabbitEventService>("RabbitMQ EventService");
                });
        }
    }
}
