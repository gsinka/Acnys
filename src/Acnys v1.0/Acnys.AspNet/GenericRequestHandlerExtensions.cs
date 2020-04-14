using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Acnys.AspNet
{
    public static class GenericRequestHandlerExtensions
    {
        public static IHostBuilder AddHttpRequestHandler(this IHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureServices((context, services) =>
            {
                Log.Verbose("Adding HTTP request handler");
                services.AddControllers().AddApplicationPart(typeof(GenericRequestHandlerController).Assembly);
            });
        }

        public static void MapHttpRequestHandler(this IEndpointRouteBuilder app, string path = null)
        {
            Log.Verbose("Mapping HTTP request handler service on path {path}", path);

            path ??= "";
            app.MapControllerRoute("generic", $"{((path ?? "").EndsWith("/") ? path : path + "/")}{{controller=GenericRequestHandler}}/{{action=post}}");
        }


    }
}
