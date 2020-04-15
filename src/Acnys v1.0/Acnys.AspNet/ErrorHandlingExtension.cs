using Microsoft.AspNetCore.Builder;

namespace Acnys.AspNet
{
    public static class ErrorHandlingExtension
    {
        public static IApplicationBuilder AddErrorHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }
}
