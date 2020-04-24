using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Acnys.AspNet
{
    public static class AspNetExtensions
    {
        public static IHostBuilder AddAspNetControllers(this IHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureServices((context, services) =>
            {
                Log.Verbose("Adding HTTP request handler");
                services.AddControllers().AddApplicationPart(typeof(GenericRequestHandlerController).Assembly);
            });
        }
    }
}