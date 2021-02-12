using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Correlation;
using Acnys.Core.Request.Abstractions;
using Acnys.Core.ValueObjects;
using Newtonsoft.Json;
using Serilog;

namespace Acnys.Core.Request.Infrastructure.Senders
{
    public class HttpCommandSender : ISendCommand
    {
        private readonly ILogger _log;
        private readonly HttpClientHandler _httpHandler;
        private readonly string _uri;

        private readonly Func<Type, string> _typeNameBuilder = type => $"{type.FullName}, {type.Assembly.GetName().Name}";

        public HttpCommandSender(ILogger log, HttpClientHandler httpHandler, string uri)
        {
            _log = log;
            _httpHandler = httpHandler;
            _uri = uri;
        }

        public async Task Send<T>(T command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default) where T : ICommand
        {
            arguments.EnrichLogContextWithCorrelation();

            _log.Debug("Sending command to HTTP endpoint {uri}", _uri.ToString());

            _log.Verbose("Command data: {@command}", command);
            _log.Verbose("Command arguments: {@arguments}", arguments);

            using var httpClient = new HttpClient(_httpHandler, false);

            var commandJson = JsonConvert.SerializeObject(command);

            httpClient.DefaultRequestHeaders.Add(RequestConstants.DomainType, _typeNameBuilder(command.GetType()));
            httpClient.DefaultRequestHeaders.Add(RequestConstants.RequestId, command.RequestId.ToString());

            if (arguments != null)
                foreach (var argument in arguments)
                {
                    httpClient.DefaultRequestHeaders.Add(argument.Key, argument.Value.ToString());
                }

            var result = await httpClient.PostAsync(
                _uri,
                new StringContent(commandJson, Encoding.UTF8, "application/json"),
                cancellationToken);

            if (!result.IsSuccessStatusCode)
            {
                _log.Error("Sending command to HTTP endpoint failed. Reason: {reason}, Response: {response}", result.ReasonPhrase, result.Content.ReadAsStringAsync());
                throw new Exceptions.HttpRequestException(result.StatusCode, await result.Content.ReadAsByteArrayAsync());
            }
        }
    }
}