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
    public class HttpCommandDispatcher : IDispatchCommand
    {
        private readonly ILogger _log;
        private readonly HttpClientHandler _clientHandler;
        private readonly Uri _uri;

        public HttpCommandDispatcher(ILogger log, HttpClientHandler clientHandler, Uri uri)
        {
            _log = log;
            _clientHandler = clientHandler;
            _uri = uri;
        }

        public async Task Dispatch<T>(T command, CancellationToken cancellationToken = default) where T : ICommand
        {
            _log.Verbose("Dispatching command {commandType} to HTTP endpoint {commandUri}", command.GetType().Name, _uri);
            
            using var client = new HttpClient(_clientHandler, false);
            var content = new StringContent(JsonConvert.SerializeObject(command), Encoding.UTF8, "application/json");
            content.Headers.Add("domain-type", command.GetType().AssemblyQualifiedName);

            var result = await client.PostAsync(_uri, content, cancellationToken);
            result.EnsureSuccessStatusCode();

            _log.Verbose("Command dispatched successfully to remote endpoint");
        }
    }
}
