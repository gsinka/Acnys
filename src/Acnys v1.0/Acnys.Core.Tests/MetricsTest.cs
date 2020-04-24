using System.Threading.Tasks;
using Acnys.AspNet;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Acnys.Core.Tests
{
    public class MetricsTest
    {
        [Theory]
        [InlineData(null)]
        [InlineData("metrics")]
        [InlineData(".well-known/metrics")]
        public async Task Metrics_listen_on_proper_endpoint(string endpoint)
        {
            var response = await BuildHost(endpoint).GetTestServer().CreateRequest(endpoint).GetAsync();
            Assert.True(response.IsSuccessStatusCode);
            
            var result = await response.Content.ReadAsStringAsync();
            Assert.StartsWith("# HELP", result);
        }

        private IHost BuildHost(string metricsPath)
        {
            var host = new HostBuilder()
                .AddAspNetControllers()
                .ConfigureWebHostDefaults(builder =>
                {
                    builder.UseTestServer();
                    builder.Configure((context, app) =>
                    {
                        app.UseRouting();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapControllers();
                            endpoints.MapHttpMetrics(metricsPath);
                        });
                    });
                })
                .Build();

            host.Start();

            return host;
        }
    }
}
