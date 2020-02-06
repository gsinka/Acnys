using System.Reflection;
using Acnys.Core.Services;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutofacSerilogIntegration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace Acnys.Core.Hosting
{
    public static class AutofacHostBuilderExtensions
    {
        public static IHostBuilder UseAutofac(this IHostBuilder hostBuilder)
        {
            return hostBuilder.UseServiceProviderFactory(context => new AutofacServiceProviderFactory());
        }

        public static IHostBuilder UseComputerClock(this IHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureContainer<ContainerBuilder>((context, builder) => builder.RegisterType<ComputerClock>().As<IClock>().SingleInstance());
        }
    }
}
