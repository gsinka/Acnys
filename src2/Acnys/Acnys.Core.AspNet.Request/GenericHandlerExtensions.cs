using Acnys.Core.Hosting.Request;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Acnys.Core.AspNet.Request
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
       
        public static void MapHttpRequestHandler(this IEndpointRouteBuilder app, string path = null)
        {
            path ??= "";
            app.MapControllerRoute("generic", $"{((path ?? "").EndsWith("/") ? path : path + "/")}{{controller=GenericRequestHandler}}/{{action=post}}");
        }
    }
}