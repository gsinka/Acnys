using System;
using System.Linq;
using System.Threading.Tasks;
using Acnys.Core;
using Acnys.Core.Infrastructure.Sender;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Acnys.AspNet
{
    public static class ResponseWriterExtension
    {
        public static async Task Write(this HttpResponse response, Exception exception)
        {
            switch (exception)
            {
                case BusinessException businessException:

                    response.StatusCode = StatusCodes.Status400BadRequest;
                    response.Headers.Add(HttpErrorConstants.HttpHeaderKey, HttpErrorConstants.BusinessError);
                    await response.WriteAsync(JsonConvert.SerializeObject(businessException));
                    break;

                case ValidationException validationException:

                    response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                    response.Headers.Add(HttpErrorConstants.HttpHeaderKey, HttpErrorConstants.ValidationError);
                    await response.WriteAsync(JsonConvert.SerializeObject(validationException));
                    break;

                default:
                    response.StatusCode = StatusCodes.Status500InternalServerError;
                    await response.WriteAsync(JsonConvert.SerializeObject(exception));
                    break;
            }
        }
    }
}