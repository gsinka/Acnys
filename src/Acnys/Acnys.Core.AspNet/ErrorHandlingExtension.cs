﻿using Microsoft.AspNetCore.Builder;

namespace Acnys.Core.AspNet
{
    public static class ErrorHandlingExtension
    {
        public static IApplicationBuilder AddErrorHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }
}
