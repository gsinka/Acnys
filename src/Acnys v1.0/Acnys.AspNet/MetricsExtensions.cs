using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Serilog;

namespace Acnys.AspNet
{
    public static class MetricsExtensions
    {
        public static void MapHttpMetrics(this IEndpointRouteBuilder app, string path = null)
        {
            Log.Verbose("Mapping Metrics endpoint path {path}", path);

            path ??= "";
            app.MapControllerRoute("metrics", $"{((path ?? "").EndsWith("/") ? path : path + "/")}{{controller=Metrics}}/{{action=getMetrics}}");
        }
    }
}
