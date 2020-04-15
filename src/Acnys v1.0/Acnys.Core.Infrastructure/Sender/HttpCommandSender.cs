﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Abstractions;
using Acnys.Core.Application.Abstractions;
using Acnys.Core.Infrastructure.Serilog;
using Acnys.Core.ValueObjects;
using FluentValidation;
using Newtonsoft.Json;
using Serilog;

namespace Acnys.Core.Infrastructure.Sender
{
    public class HttpCommandSender : ISendCommand
    {
        private readonly ILogger _log;
        private readonly HttpClient _httpClient;
        private readonly Uri _uri;

        public HttpCommandSender(ILogger log, string uri, HttpClient httpClient = null)
        {
            _log = log;
            _httpClient = httpClient ?? new HttpClient();
            _uri = new Uri(uri);
        }

        private readonly Func<Type, string> _typeNameBuilder = type => $"{type.FullName}, {type.Assembly.GetName().Name}";

        public async Task Send<T>(T command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default) where T : ICommand
        {
            arguments.EnrichLog();

            _log.Debug("Sending command to HTTP endpoint {uri}", _uri.ToString());

            _log.Verbose("Command data: {@command}", command);
            _log.Verbose("Command arguments: {@arguments}", arguments);

            var commandJson = JsonConvert.SerializeObject(command);

            _httpClient.DefaultRequestHeaders.Add(RequestConstants.DomainType, _typeNameBuilder(command.GetType()));
            _httpClient.DefaultRequestHeaders.Add(RequestConstants.RequestId, command.RequestId.ToString());

            if (arguments != null)
                foreach (var argument in arguments)
                {
                    _httpClient.DefaultRequestHeaders.Add(argument.Key, argument.Value.ToString());
                }

            var result = await _httpClient.PostAsync(
                _uri,
                new StringContent(commandJson, Encoding.UTF8, "application/json"),
                cancellationToken);

            if (!result.IsSuccessStatusCode)
            {
                _log.Error("Sending command to HTTP endpoint failed. Reason: {reason}, Response: {response}", result.ReasonPhrase, result.Content.ReadAsStringAsync());

                result.Headers.TryGetValues(HttpErrorConstants.HttpHeaderKey, out var errorTypes);
                var errorType = errorTypes?.FirstOrDefault();

                throw errorType switch
                {
                    HttpErrorConstants.BusinessError => (Exception) JsonConvert.DeserializeObject<BusinessException>(await result.Content.ReadAsStringAsync()),
                    HttpErrorConstants.ValidationError => JsonConvert.DeserializeObject<ValidationException>(await result.Content.ReadAsStringAsync()),
                    _ => new InvalidOperationException($"{(int)result.StatusCode} - {result.ReasonPhrase}")
                };
            }
        }
    }
}