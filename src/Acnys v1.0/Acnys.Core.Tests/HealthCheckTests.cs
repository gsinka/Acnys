using System.Collections.Generic;
using System.Threading.Tasks;
using Acnys.AspNet;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Acnys.Core.Tests
{

    public class HealthCheckTests
    {
        private bool _isReady;
        private bool _isLive;

        [Theory]
        [InlineData(HealthCheckConstants.ReadinessPath, HealthCheckConstants.ReadinessTag, HealthCheckConstants.LivenessPath,  HealthCheckConstants.LivenessTag)]
        [InlineData("/ready", "ready", "/live", "live")]
        public async Task Metrics_listen_on_proper_endpoint(string readinessPath, string readinessTag, string livenessPath, string livenessTag)
        {
            var server = BuildHost(readinessPath, readinessTag, livenessPath, livenessTag).GetTestServer();

            _isReady = true;
            var readiness = await server.CreateRequest(readinessPath).GetAsync();
            Assert.True(readiness.IsSuccessStatusCode);

            _isReady = false;
            readiness = await server.CreateRequest(readinessPath).GetAsync();
            Assert.False(readiness.IsSuccessStatusCode);

            _isLive = true;
            var liveness = await server.CreateRequest(livenessPath).GetAsync();
            Assert.True(liveness.IsSuccessStatusCode);

            _isLive = false;
            liveness = await server.CreateRequest(livenessPath).GetAsync();
            Assert.False(liveness.IsSuccessStatusCode);
        }


        private IHost BuildHost(string readinessPath, string readinessTag, string livenessPath, string livenessTag)
        {
            var host = new HostBuilder()
                
                .AddHealthChecks((context, builder) => builder
                    .AddCheck("live", () => _isLive ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy(), new List<string> { livenessTag })
                    .AddCheck("ready", () => _isReady ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy(), new List<string> { readinessTag })
                )

                .ConfigureServices(services => services.AddControllers() )

                .ConfigureWebHostDefaults(builder =>
                {
                    builder.UseTestServer();
                    builder.Configure((context, app) =>
                    {
                        app.UseRouting();
                        app.AddLiveness(livenessPath, livenessTag);
                        app.AddReadiness(readinessPath, readinessTag);
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapControllers();
                        });
                    });
                })
                .Build();

            host.Start();

            return host;
        }
    }
}