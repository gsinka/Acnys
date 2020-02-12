using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Acnys.Core.Hosting.HealthCheck
{
    public static class HealthCheckExtensions
    {
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
