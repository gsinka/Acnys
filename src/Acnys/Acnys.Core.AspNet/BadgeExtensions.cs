using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;

namespace Acnys.Core.AspNet
{
    public static class BadgeExtensions
    {
        public static IHostBuilder AddBadge(this IHostBuilder builder, Action<IBadgeRegistry, IServiceProvider> registrate)
        {
            return builder.ConfigureServices((context, services) => {
                services.AddSingleton((context) =>
                {
                    var badgeService = new BadgeService(context.GetService<HealthCheckService>());
                    registrate(badgeService, context);
                    return badgeService;
                });

                services.AddControllers().AddApplicationPart(typeof(BadgeController).Assembly);                
            });

        }

        public static void MapBadge(this IEndpointRouteBuilder builder, string pattern = ".badge")
        {
            Log.Verbose("Configure badge endpoint to path {path}", pattern);

            pattern ??= "";
            builder.MapControllerRoute(name: "badge", 
                                       pattern: $"{pattern.TrimEnd('/')}/{{name?}}",
                                       defaults: new { controller = "Badge", action = "Get" });
        }
    }
}
