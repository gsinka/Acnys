using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Extensions;
using Acnys.Core.Request.Abstractions;
using Acnys.Core.ValueObjects;
using Newtonsoft.Json;
using Serilog;

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
            arguments.EnrichLogContextWithCorrelation();

            _log.Debug("Sending command to HTTP endpoint {uri}", _uri.ToString());

            _log.Verbose("Command data: {@command}", command);
            _log.Verbose("Command arguments: {@arguments}", arguments);

            using var httpClient = new HttpClient(_httpHandler, false)
            {
                Timeout = TimeSpan.FromSeconds(30),
            };

            var queryJson = JsonConvert.SerializeObject(command);

            httpClient.DefaultRequestHeaders.Add(RequestConstants.DomainType, command.GetType().AssemblyQualifiedName);
            httpClient.DefaultRequestHeaders.Add(RequestConstants.RequestId, command.RequestId.ToString());

            if (arguments != null)
                foreach (var argument in arguments)
                {
                    httpClient.DefaultRequestHeaders.Add(argument.Key, argument.Value.ToString());
                }

            var result = await httpClient.PostAsync(
                _uri,
                new StringContent(queryJson, Encoding.UTF8, "application/json"),
                cancellationToken);

            if (!result.IsSuccessStatusCode)
            {
                _log.Error("Sending command to HTTP endpoint failed. Reason: {reason}, Response: {response}", result.ReasonPhrase, result.Content.ReadAsStringAsync());
                throw new Exceptions.HttpRequestException(result.StatusCode, await result.Content.ReadAsByteArrayAsync());
            }
        }

        public async Task<T> Send<T>(IQuery<T> query, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
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
            
            if (!result.IsSuccessStatusCode)
            {
                _log.Error("Sending command to HTTP endpoint failed. Reason: {reason}, Response: {response}", result.ReasonPhrase, await result.Content.ReadAsStringAsync());
                throw new Exceptions.HttpRequestException(result.StatusCode, await result.Content.ReadAsByteArrayAsync());
            }

            var responseContent = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(responseContent);
        }
    }
}