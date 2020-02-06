using Acnys.Core.Services;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Acnys.Core.Hosting
{
    public static class AutofacHostBuilderExtensions
    {
        public static IHostBuilder AddAutofac(this IHostBuilder hostBuilder)
        {
            return hostBuilder.UseServiceProviderFactory(context => new AutofacServiceProviderFactory());
        }

        public static IHostBuilder AddComputerClock(this IHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureContainer<ContainerBuilder>((context, builder) => builder.RegisterType<ComputerClock>().As<IClock>().SingleInstance());
        }
    }
}
