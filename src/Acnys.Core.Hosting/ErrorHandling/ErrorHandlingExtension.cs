using Microsoft.AspNetCore.Builder;

namespace Acnys.Core.Hosting.ErrorHandling
{
    public static class ErrorHandlingExtension
    {
        public static IApplicationBuilder AddErrorHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }
}
