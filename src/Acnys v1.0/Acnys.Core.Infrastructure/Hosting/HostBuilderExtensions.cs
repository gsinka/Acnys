using Acnys.Core.Abstractions;
using Autofac;
using Microsoft.Extensions.Hosting;

namespace Acnys.Core.Infrastructure.Hosting
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder AddClock<T>(this IHostBuilder hostBuilder) where T : IClock
        {
            hostBuilder.ConfigureContainer<ContainerBuilder>((context, builder) =>
            {
                builder.RegisterType<T>().As<IClock>().SingleInstance();
            });

            return hostBuilder;

        }
    }
}
