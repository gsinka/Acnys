using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Serilog;

namespace Acnys.AspNet
{
    public static class GenericRequestHandlerExtensions
    {
        public static void MapHttpRequestHandler(this IEndpointRouteBuilder app, string path = null)
        {
            Log.Verbose("Mapping HTTP request handler service on path {path}", path);

            path ??= "";
            app.MapControllerRoute("generic", $"{((path ?? "").EndsWith("/") ? path : path + "/")}{{controller=GenericRequestHandler}}/{{action=post}}");
        }
    }
}
