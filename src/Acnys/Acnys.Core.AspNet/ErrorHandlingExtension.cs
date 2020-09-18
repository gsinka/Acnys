using Acnys.Core.AspNet.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace Acnys.Core.AspNet
{
    public static class ErrorHandlingExtension
    {
        public static IApplicationBuilder AddErrorHandling(this IApplicationBuilder app)
        {
            app.UseMiddleware<ErrorHandlingMiddleware>();
            return app.UseMiddleware<ErrorMetricsMiddleware>();
        }
    }
}
