using Acnys.Core.Testing;
using Autofac;
using Microsoft.Extensions.Hosting;

namespace Acnys.Core.Testing
{
    public static class EventAwaiterExtensions
    {
        public static IHostBuilder AddEventAwaiter(this IHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureContainer<ContainerBuilder>((context, builder) =>
            {
                builder.RegisterType<EventAwaiter>().AsImplementedInterfaces().AsSelf().SingleInstance();
            });
        }
    }
}
