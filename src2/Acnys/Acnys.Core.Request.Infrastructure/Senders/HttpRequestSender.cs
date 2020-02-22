using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Abstractions.Extensions;
using Acnys.Core.Request.Abstractions;
using Newtonsoft.Json;
using Serilog;
using Serilog.Context;

namespace Acnys.Core.Request.Infrastructure.Senders
{
    public class HttpRequestSender : ISendRequest
    {
        private readonly ILogger _log;
        private readonly HttpClientHandler _httpHandler;
        private readonly Uri _uri;
        
        public HttpRequestSender(ILogger log, HttpClientHandler httpHandler, string uri)
        {
            _log = log;
            _httpHandler = httpHandler;
            _uri = new Uri(uri);
        }

        public async Task Send<T>(T command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default) where T : ICommand
        {
            //using var correlationId = LogContext.PushProperty("correlationId", arguments.CorrelationId());
            //using var causationId = LogContext.PushProperty("causationId", arguments.CorrelationId());

            _log.Debug("Sending command to HTTP endpoint {uri}", _uri.ToString());

            _log.Verbose("Command data: {@command}", command);
            _log.Verbose("Command arguments: {@arguments}", arguments);

            using var httpClient = new HttpClient(_httpHandler, false)
            {
                Timeout = TimeSpan.FromMinutes(5),
            };

            var queryJson = JsonConvert.SerializeObject(command);

            httpClient.DefaultRequestHeaders.Add("domain-type", command.GetType().AssemblyQualifiedName);

            if (arguments != null)
                foreach (var argument in arguments)
                {
                    httpClient.DefaultRequestHeaders.Add(argument.Key, argument.Value.ToString());
                }

            var result = await httpClient.PostAsync(
                _uri,
                new StringContent(queryJson, Encoding.UTF8, "application/json"),
                cancellationToken);

            result.EnsureSuccessStatusCode();
        }

        public async Task<T> Send<T>(IQuery<T> query, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            //using var correlationId = LogContext.PushProperty("correlationId", arguments.CorrelationId());
            //using var causationId = LogContext.PushProperty("causationId", arguments.CorrelationId());

            _log.Debug("Sending query to HTTP endpoint {uri}", _uri.ToString());

            _log.Verbose("Query data: {@query}", query);
            _log.Verbose("Query arguments: {@query}", arguments);

            using var httpClient = new HttpClient(_httpHandler, false)
            {
                Timeout = TimeSpan.FromMinutes(5),
            };

            var queryJson = JsonConvert.SerializeObject(query);
            
            httpClient.DefaultRequestHeaders.Add("domain-type", query.GetType().AssemblyQualifiedName);

            if (arguments != null)
                foreach (var argument in arguments)
                {
                    httpClient.DefaultRequestHeaders.Add(argument.Key, argument.Value.ToString());
                }

            var result = await httpClient.PostAsync(
                _uri, 
                new StringContent(queryJson, Encoding.UTF8, "application/json"), 
                cancellationToken);

            result.EnsureSuccessStatusCode();

            var responseContent = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(responseContent);
        }
    }
}