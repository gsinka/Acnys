using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Abstractions;
using Acnys.Core.Application.Abstractions;
using Acnys.Core.Infrastructure.Abstractions;
using Autofac;
using Serilog;

namespace Acnys.Core.Infrastructure.Request
{
    public class QueryDispatcher : IDispatchQuery
    {
        private readonly ILifetimeScope _scope;
        private readonly ILogger _log;

        public QueryDispatcher(ILifetimeScope scope, ILogger log)
        {
            _scope = scope;
            _log = log;
        }

        /// <summary>
        /// Dispatch query
        /// </summary>
        /// <typeparam name="TResult">Type of query result</typeparam>
        /// <param name="query">Query object</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public async Task<TResult> Dispatch<TResult>(IQuery<TResult> query, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            var queryName = query.GetType().Name;

            _log.Verbose("Dispatching query {queryName}", queryName);

            _log.Verbose("Query object: {@query}", query);

            //using var scope = _scope.BeginLifetimeScope();

            _log.Verbose("Lifetime scope {scopeId} created for query", _scope.GetHashCode());

            try
            {
                var queryType = query.GetType();
                var requestedHandlerType = typeof(IHandleQuery<,>).MakeGenericType(query.GetType(), typeof(TResult));
                _log.Verbose("Looking handler with type {handlerType}", requestedHandlerType);

                var handler = (dynamic) _scope.Resolve(requestedHandlerType);
                Type handlerType = handler.GetType();

                _log.Verbose("Handling {queryType} with {handlerType}", queryType.Name, handlerType.Name);
                TResult result = await handler.Handle((dynamic) query, arguments, cancellationToken);

                _log.Debug("Query dispatch completed successfully");
                _log.Verbose("Query result object: {@queryResult}", result);

                return result;
            }
            catch (Exception exception)
            {
                _log.Error(exception, "Query dispatch failed");
                throw;
            }
            finally
            {
                _log.Verbose("Ending lifetime scope {scopeId}", _scope.GetHashCode());
            }
        }
    }
}
