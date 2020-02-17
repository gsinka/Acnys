using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Request.Abstractions;
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

            var json = await reader.ReadToEndAsync();
            if (string.IsNullOrEmpty(json)) json = "{}";

            var domainType = Request.Headers["domain-type"];

            dynamic request = string.IsNullOrEmpty(domainType)
                ? JsonConvert.DeserializeObject(json, _jsonSerializerSettings) ?? throw new InvalidOperationException("Unknown request")
                : JsonConvert.DeserializeObject(json, Type.GetType(domainType));

            Type requestType = request.GetType();

            var arguments = Request.Headers.Keys.ToDictionary<string, string, object>(key => key, key => Request.Headers[key]);

            if (request is ICommand)
            {
                await _commandDispatcher.Dispatch(request, arguments, cancellationToken);
                return Ok();
            }

            if (requestType.GetInterfaces().Any(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IQuery<>)))
            {
                var result = await _queryDispatcher.Dispatch(request, arguments, cancellationToken);
                return Ok(result);
            }

            throw new InvalidOperationException("Unknown request type: {requestType}", request.GetType().ToString());
        }
    }
}
