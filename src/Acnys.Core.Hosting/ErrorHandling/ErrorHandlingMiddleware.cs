using Acnys.Core.Constants;
using Acnys.Core.Exceptions;
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
                await context.Response.WriteAsync(JsonConvert.SerializeObject(ComposeResponse(exception)));
            }
            catch (BusinessException exception)
            {
                _logger.Error(exception, "Error occured during process");

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = MediaTypeNames.Application.Json;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(ComposeResponse(exception)));
            }
            catch (ValidationException exception)
            {
                _logger.Error(exception, "Validation error");

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = MediaTypeNames.Application.Json;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(ComposeResponse(exception)));
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Error occured during process.");

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = MediaTypeNames.Application.Json;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(ComposeResponse(exception)));
            }
        }

        private string ComposeResponse(BusinessException exception)
        {
            return JsonConvert.SerializeObject(new
            {
                Code = exception.ErrorCode,
                exception.Message
            });
        }

        private string ComposeResponse(Exception exception)
        {
            return JsonConvert.SerializeObject(new
            {
                Code = ErrorCodes.UnknowError,
                exception.Message
            });
        }

        private string ComposeResponse(ValidationException exception)
        {
            return JsonConvert.SerializeObject(new
            {
                Code = ErrorCodes.ValidationError,
                exception.Message,
                ValidationErrors = exception.Errors.Select(e => new { Code = e.ErrorCode, Message = e.ErrorMessage, Property = e.PropertyName })
            });
        }
    }
}
