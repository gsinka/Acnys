using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Request;
using Microsoft.AspNetCore.Server.IIS;
using Newtonsoft.Json;
using Serilog;

namespace Acnys.Core.Hosting.Request.Sender
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

        public async Task Send<T>(T command, CancellationToken cancellationToken = default) where T : ICommand
        {
            _log.Debug("Sending command to HTTP endpoint {uri}", _uri.ToString());

            using var httpClient = new HttpClient(_httpHandler, false)
            {
                Timeout = TimeSpan.FromMinutes(5),
            };

            var queryJson = JsonConvert.SerializeObject(command);

            httpClient.DefaultRequestHeaders.Add("domain-type", command.GetType().AssemblyQualifiedName);

            var result = await httpClient.PostAsync(
                _uri,
                new StringContent(queryJson, Encoding.UTF8, "application/json"),
                cancellationToken);

            if (!result.IsSuccessStatusCode)
                _log.Error("Sending command to HTTP endpoint failed. Reason: {reason}, Response: {response}", result.ReasonPhrase, result.Content.ReadAsStringAsync());

            result.EnsureSuccessStatusCode();
        }

        public async Task<T> Send<T>(IQuery<T> query, CancellationToken cancellationToken = default)
        {
            _log.Debug("Sending query to HTTP endpoint {uri}", _uri.ToString());

            using var httpClient = new HttpClient(_httpHandler, false)
            {
                Timeout = TimeSpan.FromMinutes(5),
            };

            var queryJson = JsonConvert.SerializeObject(query);
            
            httpClient.DefaultRequestHeaders.Add("domain-type", query.GetType().AssemblyQualifiedName);
            
            var result = await httpClient.PostAsync(
                _uri, 
                new StringContent(queryJson, Encoding.UTF8, "application/json"), 
                cancellationToken);

            if (!result.IsSuccessStatusCode)
                _log.Error("Sending query to HTTP endpoint failed. Reason: {reason}, Response: {response}", result.ReasonPhrase, result.Content.ReadAsStringAsync());

            result.EnsureSuccessStatusCode();

            var responseContent = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(responseContent);
        }
    }
}