using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Request.Abstractions;
using Newtonsoft.Json;
using Serilog;

namespace Acnys.Core.Request
{
    public class HttpQueryDispatcher : IDispatchQuery
    {
        private readonly ILogger _log;
        private readonly HttpClientHandler _clientHandler;
        private readonly Uri _uri;

        public HttpQueryDispatcher(ILogger log, HttpClientHandler clientHandler, Uri uri)
        {
            _log = log;
            _clientHandler = clientHandler;
            _uri = uri;
        }

        public async Task<TResult> Dispatch<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
        {
            _log.Verbose("Dispatching query {queryType} to HTTP endpoint {queryUri}", query.GetType().Name, _uri);

            using var client = new HttpClient(_clientHandler, false);
            
            var content = new StringContent(JsonConvert.SerializeObject(query), Encoding.UTF8, "application/json");
            content.Headers.Add("domain-type", query.GetType().AssemblyQualifiedName);

            var result = await client.PostAsync(_uri, content, cancellationToken);
            result.EnsureSuccessStatusCode();

            var responseJson = await result.Content.ReadAsStringAsync();
            var queryResult = JsonConvert.DeserializeObject<TResult>(responseJson);

            _log.Verbose("Query dispatched successfully to remote endpoint");

            return queryResult;
        }
    }
}