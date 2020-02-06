using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Acnys.Core.Hosting.Request
{
    public static class GenericHandlerExtensions
    {
        public static IHostBuilder AddHttpRequestHandler(this IHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureServices((context, services) =>
            {
                services.AddControllers().AddApplicationPart(typeof(GenericRequestHandlerController).Assembly);
            });
        }

        public static IApplicationBuilder UseHttpRequestHandler(this IApplicationBuilder app, string path = null)
        {
            path ??= "";

            app.UseEndpoints(routeBuilder =>
                {
                    routeBuilder.MapControllerRoute("generic", $"{((path ?? "").EndsWith("/") ? path : path + "/")}{{controller=GenericRequestHandler}}/{{action=post}}");
                });
            
            return app;
        }


    }
}