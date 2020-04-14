using System;
using Autofac;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Acnys.Core.Infrastructure.Serilog
{
    public static class SerilogHostBuilderExtensions
    {
        public static IHostBuilder AddSerilog(this IHostBuilder hostBuilder, Action<HostBuilderContext, LoggerConfiguration> config, bool preserveStaticLogger = false, bool writeToProviders = false)
        {
            return hostBuilder
                    .ConfigureContainer<ContainerBuilder>((context, builder) =>
                    {
                        var logConfig = new LoggerConfiguration();
                        config(context, logConfig);
                        Log.Logger = logConfig.CreateLogger();
                        builder.RegisterLogger();
                    })
                    .UseSerilog()
                ;
        }
    }
}