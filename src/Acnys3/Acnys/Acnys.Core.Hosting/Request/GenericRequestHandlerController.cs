using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Request;
using Acnys.Core.Request.Application;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Acnys.Core.Hosting.Request
{
    [Produces("application/json")]
    public class GenericRequestHandlerController : ControllerBase
    {
        private readonly IDispatchCommand _commandDispatcher;
        private readonly IDispatchQuery _queryDispatcher;
      
        private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };

        public GenericRequestHandlerController(IDispatchCommand commandDispatcher, IDispatchQuery queryDispatcher)
        {
            _commandDispatcher = commandDispatcher;
            _queryDispatcher = queryDispatcher;
        }

        [HttpPost]
        public async Task<IActionResult> Post(CancellationToken cancellationToken = default)
        {
            using var reader = new StreamReader(Request.Body, Encoding.UTF8);

            try
            {
                var json = await reader.ReadToEndAsync();
                if (string.IsNullOrEmpty(json)) json = "{}";

                var domainType = Request.Headers["domain-type"];

                dynamic request = string.IsNullOrEmpty(domainType)
                    ? JsonConvert.DeserializeObject(json, _jsonSerializerSettings) ?? throw new InvalidOperationException("Unknown request")
                    : JsonConvert.DeserializeObject(json, Type.GetType(domainType));

                Type requestType = request.GetType();

                if (request is ICommand)
                {
                    await _commandDispatcher.Dispatch(request, cancellationToken);
                    return Ok();
                }
                else if (requestType.GetInterfaces().Any(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IQuery<>)))
                {
                    var result = await _queryDispatcher.Dispatch(request, cancellationToken);
                    return Ok(result);
                } 
                else
                {
                    throw new InvalidOperationException("Unknown request type: {requestType}", request.GetType().ToString());
                }
            }
            catch (Exception exception)
            {
                return BadRequest(exception.Message);
            }
        }
    }
}
