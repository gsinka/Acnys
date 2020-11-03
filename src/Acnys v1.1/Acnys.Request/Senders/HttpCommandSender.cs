using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Request.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Acnys.Request.Senders
{
    public delegate bool CommandFilterDelegate(ICommand command, IDictionary<string, object> arguments);

    public class HttpCommandSender : ISendCommand
    {
        private readonly ILogger<HttpCommandSender> _log;
        private readonly CommandFilterDelegate _filter;
        private readonly Func<ISendCommand> _next;
        private readonly Uri _endpoint;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Func<ICommand, IDictionary<string, object>, HttpContent> _contentBuilder;

        public HttpCommandSender(ILogger<HttpCommandSender> log, CommandFilterDelegate filter, Func<ISendCommand> next, Uri endpoint, IHttpClientFactory httpClientFactory, Func<ICommand, IDictionary<string, object>, HttpContent> contentBuilder)
        {
            _log = log;
            _filter = filter;
            _next = next;
            _endpoint = endpoint;
            _httpClientFactory = httpClientFactory;
            _contentBuilder = contentBuilder;
        }

        public async Task SendAsync<T>(T command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default) where T : ICommand
        {
            if (_filter(command, arguments))
            {
                var client = _httpClientFactory.CreateClient("HttpCommandSender");
                var content = 
                await client.PostAsync(_endpoint, _contentBuilder(command, arguments));
            }
            else
            {
                _log.LogTrace("Http command sender rejected command {commandType}", command.GetType().Name);
                await _next().SendAsync(command, arguments, cancellationToken);
            }
        }
    }
}
