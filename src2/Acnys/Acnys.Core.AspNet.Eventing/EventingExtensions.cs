using Acnys.Core.Eventing.Infrastructure.Extensions;
using Autofac;
using Microsoft.Extensions.Hosting;

namespace Acnys.Core.AspNet.Eventing
{
    public static class EventingExtensions
    {
        public static IHostBuilder AddEventing(this IHostBuilder builder)
        {
            builder.ConfigureContainer<ContainerBuilder>((context, containerBuilder) =>
                {
                    containerBuilder.RegisterEventDispatcher();
                });

            return builder;
        }

        public static IHostBuilder RegisterEventHandlersFromAssemblyOf<T>(this IHostBuilder builder)
        {
            builder.ConfigureContainer<ContainerBuilder>((context, containerBuilder) =>
                {
                    containerBuilder.RegisterEventHandlersFromAssemblyOf<T>();
                });

            return builder;
        }
    }
}
