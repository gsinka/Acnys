using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Abstractions.Extensions;
using Acnys.Core.Request.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;
using Serilog.Context;

namespace Acnys.Core.AspNet.Request
{
    [Produces("application/json")]
    public class GenericRequestHandlerController : ControllerBase
    {
        private readonly IDispatchCommand _commandDispatcher;
        private readonly IDispatchQuery _queryDispatcher;
        private readonly ILogger _log;

        private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings()
            {TypeNameHandling = TypeNameHandling.All};

        public GenericRequestHandlerController(IDispatchCommand commandDispatcher, IDispatchQuery queryDispatcher,
            ILogger log)
        {
            _commandDispatcher = commandDispatcher;
            _queryDispatcher = queryDispatcher;
            _log = log;
        }

        [HttpPost]
        public async Task<IActionResult> Post(CancellationToken cancellationToken = default)
        {
            _log.Debug("Handling incoming request with generic request handler");

            using var reader = new StreamReader(Request.Body, Encoding.UTF8);

            var json = await reader.ReadToEndAsync();
            if (string.IsNullOrEmpty(json)) json = "{}";

            var domainType = Request.Headers[HttpRequestHeaderConstants.DomainType];

            dynamic request = string.IsNullOrEmpty(domainType)
                ? JsonConvert.DeserializeObject(json, _jsonSerializerSettings) ??
                  throw new InvalidOperationException("Unknown request")
                : JsonConvert.DeserializeObject(json, Type.GetType(domainType));

            Type requestType = request.GetType();

            _log.Debug("Generic request identified as {requestType}", requestType.FullName);

            var arguments =
                Request.Headers.Keys.ToDictionary<string, string, object>(key => key, key => Request.Headers[key]);

            using var correlationId = LogContext.PushProperty(HttpRequestHeaderConstants.CorrelationId, arguments.CorrelationId());
            using var causationId = LogContext.PushProperty(HttpRequestHeaderConstants.CausationId, arguments.CausationId());

            _log.Verbose("Arguments parsed from request header: {@arguments}", arguments);

            if (request is ICommand)
            {
                await _commandDispatcher.Dispatch(request, arguments, cancellationToken);
                return Ok();
            }

            if (requestType.GetInterfaces()
                .Any(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IQuery<>)))
            {
                var result = await _queryDispatcher.Dispatch(request, arguments, cancellationToken);
                return Ok(result);
            }

            throw new InvalidOperationException("Unknown request type: {requestType}", request.GetType().ToString());
        }
    }
}
