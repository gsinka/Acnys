using System;
using System.Collections.Generic;
using System.Text;
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
    }
}
