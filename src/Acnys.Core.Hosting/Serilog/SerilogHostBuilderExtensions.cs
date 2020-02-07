using System;
using Autofac;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Acnys.Core.Hosting.Serilog
{
    public static class SerilogHostBuilderExtensions
    {
        public static IHostBuilder AddSerilog(this IHostBuilder hostBuilder, Action<HostBuilderContext, LoggerConfiguration> config, bool preserveStaticLogger = false, bool writeToProviders = false)
        {
            return hostBuilder
                    .ConfigureContainer<ContainerBuilder>((context, builder) => builder.RegisterLogger())
                    .UseSerilog(config, preserveStaticLogger, writeToProviders)
                ;
        }
    }
}