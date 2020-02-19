using Acnys.Core.Constants;
using Acnys.Core.Exceptions;
using Acnys.Core.Hosting.ErrorHandling.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Acnys.Core.Hosting.ErrorHandling
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
            catch (BusinessException exception) when (exception.ErrorCode == "404")
            {
                _logger.Error(exception, "Resource not found.");

                context.Response.StatusCode = StatusCodes.Status404NotFound;
                context.Response.ContentType = MediaTypeNames.Application.Json;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(new ErrorResponse(exception.ErrorCode, exception.Message)));
            }
            catch (BusinessException exception)
            {
                _logger.Error(exception, "Error occured during process");

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = MediaTypeNames.Application.Json;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(new ErrorResponse(exception.ErrorCode, exception.Message)));
            }
            catch (ValidationException exception)
            {
                _logger.Error(exception, "Validation error");

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = MediaTypeNames.Application.Json;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(new ValidationErrorResponse(ErrorCodes.ValidationError, exception.Errors.Select(e => new ValidationError(e.ErrorCode, e.ErrorMessage, e.PropertyName)))));
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Error occured during process.");

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = MediaTypeNames.Application.Json;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(new ErrorResponse(ErrorCodes.UnknowError, exception.Message)));
            }
        }
    }
}
