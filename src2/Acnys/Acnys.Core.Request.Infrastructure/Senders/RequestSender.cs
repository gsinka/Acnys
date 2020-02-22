using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Abstractions.Extensions;
using Acnys.Core.Request.Abstractions;
using Autofac.Features.Indexed;
using Serilog;
using Serilog.Context;

namespace Acnys.Core.Request.Infrastructure.Senders
{
    public class RequestSender : ISendRequest
    {
        private readonly ILogger _log;
        private readonly IIndex<object, ISendRequest> _requestSenderSelector;
        private readonly Func<IRequest, object> _senderKeySelector;

        public RequestSender(ILogger log, IIndex<object, ISendRequest> requestSenderSelector, Func<IRequest, object> senderKeySelector)
        {
            _log = log;
            _requestSenderSelector = requestSenderSelector;
            _senderKeySelector = senderKeySelector;
        }

        public async Task Send<T>(T command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default) where T : ICommand
        {
            using var correlationId = LogContext.PushProperty("correlationId", arguments.CorrelationId());
            using var causationId = LogContext.PushProperty("causationId", arguments.CausationId());

            var senderKey = _senderKeySelector(command);
            _log.Verbose("Sender key {senderKey} resolved for command {commandType}", senderKey, command.GetType().Name);

            var sender = _requestSenderSelector[senderKey];
            _log.Verbose("Sender {senderType} resolved for command {commandType}", sender.GetType().Name, command.GetType().Name);

            await sender.Send(command, arguments, cancellationToken);
        }

        public async Task<T> Send<T>(IQuery<T> query, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            using var correlationId = LogContext.PushProperty("correlationId", arguments.CorrelationId());
            using var causationId = LogContext.PushProperty("causationId", arguments.CausationId());

            var senderKey = _senderKeySelector(query);
            _log.Verbose("Sender key {senderKey} resolved for query {queryType}", senderKey, query.GetType().Name);

            var sender = _requestSenderSelector[senderKey];
            _log.Verbose("Sender {senderType} resolved for query {queryType}", sender.GetType().Name, query.GetType().Name);

            return await sender.Send(query, arguments, cancellationToken);
        }
    }
}