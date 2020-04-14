using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Acnys.Core.Infrastructure.Hosting
{
    public static class AutofacHostBuilderExtensions
    {
        public static IHostBuilder AddAutofac(this IHostBuilder hostBuilder)
        {
            Log.Verbose("Adding Autofac service provider factory");
            return hostBuilder.UseServiceProviderFactory(context => new AutofacServiceProviderFactory());
        }
    }
}
