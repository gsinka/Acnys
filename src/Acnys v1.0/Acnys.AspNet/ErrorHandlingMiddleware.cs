using System;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace Acnys.AspNet
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _request;
        private readonly ILogger _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger logger)
        {
            _request = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _request.Invoke(context);
            }

            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                await context.Response.Write(exception);
            }
        }
    }
}
