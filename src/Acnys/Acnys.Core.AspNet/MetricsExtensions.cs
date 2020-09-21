using Jaeger;
using Jaeger.Reporters;
using Jaeger.Samplers;
using Jaeger.Senders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTracing;
using OpenTracing.Util;
using Serilog;
using System;
using System.Reflection;

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
