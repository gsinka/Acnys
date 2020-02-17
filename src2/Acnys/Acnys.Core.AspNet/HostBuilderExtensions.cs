using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Acnys.Core.AspNet
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder AddAutofac(this IHostBuilder hostBuilder)
        {
            return hostBuilder.UseServiceProviderFactory(context => new AutofacServiceProviderFactory());
        }
    }
}
