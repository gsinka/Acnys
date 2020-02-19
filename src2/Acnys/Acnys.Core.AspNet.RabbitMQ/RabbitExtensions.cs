using System;
using Acnys.Core.Eventing.Abstractions;
using Autofac;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Acnys.Core.AspNet.RabbitMQ
{
    public static class RabbitExtensions
    {
        public static IHostBuilder AddRabbitService(this IHostBuilder hostBuilder, Action<HostBuilderContext, RabbitServiceConfiguration> config)
        {
            return hostBuilder.ConfigureContainer<ContainerBuilder>((context, builder) =>
            {
                var rabbitConfig = new RabbitServiceConfiguration();
                config(context, rabbitConfig);

                Log.Verbose("Adding RabbitMQ service using {rabbitUri}", rabbitConfig.Uri);

                builder.Register(componentContext =>
                        new RabbitHostedService(componentContext.Resolve<ILogger>().ForContext<RabbitHostedService>(),
                            componentContext.Resolve<IDispatchEvent>(), rabbitConfig))
                    .AsImplementedInterfaces().SingleInstance();

                //builder.RegisterType<RabbitHostedService>().AsImplementedInterfaces().SingleInstance();
            });
        }
    }
}
