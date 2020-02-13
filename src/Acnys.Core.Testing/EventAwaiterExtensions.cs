using System;
using Acnys.Core.Request.Application;
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
                builder.RegisterGenericDecorator(typeof(EventAwaiterDecorator<>), typeof(IHandleEvent<>));
                builder.RegisterGeneric(typeof(EventAwaiter<>)).AsSelf().SingleInstance();
            });
        }
    }
}
