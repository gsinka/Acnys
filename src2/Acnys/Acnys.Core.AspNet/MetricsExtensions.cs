using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Acnys.Core.AspNet
{
    public static class MetricsExtensions
    {
        public static IHostBuilder AddHttpMetrics(this IHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureServices((context, services) =>
            {
                Log.Verbose("Adding HTTP metrics service");
                services.AddControllers().AddApplicationPart(typeof(MetricsController).Assembly);
            });
        }
    }
}
