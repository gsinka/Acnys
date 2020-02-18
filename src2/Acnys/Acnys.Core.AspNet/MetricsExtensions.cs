using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Acnys.Core.AspNet
{
    public static class MetricsExtensions
    {
        public static IHostBuilder AddHttpMetrics(this IHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureServices((context, services) =>
            {
                services.AddControllers().AddApplicationPart(typeof(MetricsController).Assembly);
            });
        }
    }
}
