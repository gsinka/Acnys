using Acnys.Core.AspNet.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace Acnys.Core.AspNet
{
    public static class ErrorHandlingExtension
    {
        public static IApplicationBuilder AddErrorHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ErrorHandlingMiddleware>();
        }
        public static IApplicationBuilder AddErrorMetricsMiddleware( this IApplicationBuilder app)
        {
            return app.UseMiddleware<ErrorMetricsMiddleware>();
        }
    }
}
