using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Acnys.Core.AspNet
{
    public static class HealthCheckExtensions
    {
        public static IHostBuilder AddHealthChecks(this IHostBuilder hostBuilder, Action<HostBuilderContext, IHealthChecksBuilder> config)
        {
            return hostBuilder.ConfigureServices((context, services) => { config(context, services.AddHealthChecks()); });
        }

        public static IApplicationBuilder AddReadiness(this IApplicationBuilder app)
        {
            return app.UseHealthChecks("/.well-known/ready", new HealthCheckOptions
            {
                Predicate = registration =>
                    registration.Tags == null || !registration.Tags.Any() ||
                    registration.Tags.Contains("Readiness")
            });

        }

        public static IApplicationBuilder AddLiveness(this IApplicationBuilder app)
        {
            return app.UseHealthChecks("/.well-known/live", new HealthCheckOptions
            {
                Predicate = registration =>
                    registration.Tags == null || !registration.Tags.Any() ||
                    registration.Tags.Contains("Liveness")
            });
        }
    }
}
