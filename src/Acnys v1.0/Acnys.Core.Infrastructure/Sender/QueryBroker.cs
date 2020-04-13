using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Abstractions;
using Acnys.Core.Application.Abstractions;
using Autofac.Features.Indexed;
using Serilog;

namespace Acnys.Core.Infrastructure.Sender
{
    public class QueryBroker : ISendQuery
    {
        private readonly ILogger _log;
        private readonly Func<IQuery<dynamic>, IDictionary<string, object>, object> _senderKeySelector;
        private readonly IIndex<object, ISendQuery> _senderResolver;


        public QueryBroker(ILogger log, Func<IQuery<dynamic>, IDictionary<string, object>, object> senderKeySelector, IIndex<object, ISendQuery> senderResolver)
        {
            _log = log;
            _senderKeySelector = senderKeySelector;
            _senderResolver = senderResolver;
        }

        public async Task<T> Send<T>(IQuery<T> query, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            arguments.EnrichLog();

            var senderKey = _senderKeySelector(query as IQuery<dynamic>, arguments);

            if (senderKey == null)
            {
                throw new InvalidOperationException($"Unable to resolve sender key for command {query.GetType().Name}");
            }

            _log.Verbose("Sender key {senderKey} resolved for command {commandType}", senderKey, query.GetType().Name);


            if (!_senderResolver.TryGetValue(senderKey, out var sender))
            {
                throw new InvalidOperationException($"Unable to resolve sender for key {senderKey}");
            }

            _log.Verbose("Sender {senderType} resolved for command {commandType}", sender.GetType().Name, query.GetType().Name);

            return await sender.Send(query, arguments, cancellationToken);
        }
    }
}