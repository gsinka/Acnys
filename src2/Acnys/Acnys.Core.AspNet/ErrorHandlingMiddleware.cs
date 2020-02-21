using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Acnys.Core.Exceptions;
using Acnys.Core.ValueObjects;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Serilog;

namespace Acnys.Core.AspNet
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
            catch (BusinessException exception) when (exception.ErrorCode == ErrorCode.NotFound.Id)
            {
                _logger.Error(exception, ErrorCode.NotFound.Message);

                context.Response.StatusCode = StatusCodes.Status404NotFound;
                context.Response.ContentType = MediaTypeNames.Application.Json;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(ComposeResponse(exception)));
            }
            catch (BusinessException exception)
            {
                _logger.Error(exception, ErrorCode.Business.Message);

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = MediaTypeNames.Application.Json;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(ComposeResponse(exception)));
            }
            catch (ValidationException exception)
            {
                _logger.Error(exception, ErrorCode.Validation.Message);

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = MediaTypeNames.Application.Json;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(ComposeResponse(exception)));
            }
            catch (Exception exception)
            {
                _logger.Error(exception, ErrorCode.Unknown.Message);

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
                Code = ErrorCode.Unknown,
                exception.Message
            });
        }

        private string ComposeResponse(ValidationException exception)
        {
            return JsonConvert.SerializeObject(new
            {
                Code = ErrorCode.Validation,
                exception.Message,
                ValidationErrors = exception.Errors.Select(e => new { Code = e.ErrorCode, Message = e.ErrorMessage, Property = e.PropertyName })
            });
        }
    }
}
