using Acnys.Core.Eventing.Infrastructure.Extensions;
using Autofac;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Acnys.Core.AspNet.Eventing
{
    public static class EventingExtensions
    {
        public static IHostBuilder AddEventing(this IHostBuilder builder)
        {
            builder.ConfigureContainer<ContainerBuilder>((context, containerBuilder) =>
                {
                    Log.Verbose("Registering event dispatcher");
                    containerBuilder.RegisterEventDispatcher();
                });

            return builder;
        }
        public static IHostBuilder AddFullEventingMetrics(this IHostBuilder builder)
        {
            builder.ConfigureContainer<ContainerBuilder>((context, containerBuilder) =>
            {
                Log.Verbose("Registering eventing metrics");
                containerBuilder.AddMetricsService();
                containerBuilder.AddEventCountMetricsBehaviour();
                containerBuilder.AddEventDurationMetricsBehaviour();
            });

            return builder;
        }
        public static IHostBuilder RegisterEventHandlersFromAssemblyOf<T>(this IHostBuilder builder)
        {
            builder.ConfigureContainer<ContainerBuilder>((context, containerBuilder) =>
                {
                    Log.Verbose("Registering event handlers from assembly {assembly}", typeof(T).Assembly.FullName);
                    containerBuilder.RegisterEventHandlersFromAssemblyOf<T>();
                });

            return builder;
        }
    }
}
