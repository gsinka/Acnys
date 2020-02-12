using Microsoft.Extensions.DependencyInjection;

namespace Acnys.Core.Hosting.Metrics
{
    public static class MetricsExtensions
    {
        public static void AddHttpMetrics(this IServiceCollection services)
        {
            services.AddControllers().AddApplicationPart(typeof(MetricsController).Assembly);
        }
    }
}
