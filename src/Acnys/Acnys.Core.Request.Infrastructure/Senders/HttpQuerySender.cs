using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Request.Abstractions;
using Acnys.Core.ValueObjects;
using Newtonsoft.Json;
using Serilog;

namespace Acnys.Core.Request.Infrastructure.Senders
{
    public class HttpQuerySender : ISendQuery
    {
        private readonly ILogger _log;
        private readonly HttpClientHandler _httpHandler;
        private readonly string _uri;

        private readonly Func<Type, string> _typeNameBuilder = type => $"{type.FullName}, {type.Assembly.GetName().Name}";

        public HttpQuerySender(ILogger log, HttpClientHandler httpHandler, string uri)
        {
            _log = log;
            _httpHandler = httpHandler;
            _uri = uri;
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

            httpClient.DefaultRequestHeaders.Add(RequestConstants.DomainType, _typeNameBuilder(query.GetType()));
            httpClient.DefaultRequestHeaders.Add(RequestConstants.RequestId, query.RequestId.ToString());

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