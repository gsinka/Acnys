using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Acnys.AspNet
{
    public static class HealthCheckExtensions
    {
        public static IHostBuilder AddHealthChecks(this IHostBuilder hostBuilder, Action<HostBuilderContext, IHealthChecksBuilder> config)
        {
            return hostBuilder.ConfigureServices((context, services) => { config(context, services.AddHealthChecks()); });
        }

        public static IApplicationBuilder AddReadiness(this IApplicationBuilder app, string readinessPath = HealthCheckConstants.ReadinessPath, string readinessTag = HealthCheckConstants.ReadinessTag)
        {
            return app.UseHealthChecks(readinessPath, new HealthCheckOptions
            {
                Predicate = registration => registration.Tags == null || !registration.Tags.Any() || registration.Tags.Contains(readinessTag)
            });
        }

        public static IApplicationBuilder AddLiveness(this IApplicationBuilder app, string livenessPath = HealthCheckConstants.LivenessPath, string livenessTag = HealthCheckConstants.LivenessTag)
        {
            return app.UseHealthChecks(livenessPath, new HealthCheckOptions
            {
                Predicate = registration => registration.Tags == null || !registration.Tags.Any() || registration.Tags.Contains(livenessTag)
            });
        }
    }

    public static class HealthCheckConstants
    {
        public const string ReadinessTag = "Readiness";
        public const string ReadinessPath = "/.well-known/ready";
        public const string LivenessTag = "Liveness";
        public const string LivenessPath = "/.well-known/live";
    }
}
