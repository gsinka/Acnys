using System;
using Autofac;
using Microsoft.Extensions.Hosting;

namespace Acnys.Core.Hosting.Events
{
    public static class HostBuilderEventExtensions
    {
        public static IHostBuilder AddEvents(this IHostBuilder hostBuilder, Action<HostBuilderContext, EventsBuilder> eventsBuilder)
        {
            return hostBuilder.ConfigureContainer<ContainerBuilder>((context, builder) =>
            {
                eventsBuilder(context, new EventsBuilder(context, builder));
            });
        }

        public static IHostBuilder AddRequestService(this IHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureContainer<ContainerBuilder>((context, builder) =>
            {
                builder.RegisterType<RequestService>().AsImplementedInterfaces().SingleInstance();
            });
        }
    }
}
