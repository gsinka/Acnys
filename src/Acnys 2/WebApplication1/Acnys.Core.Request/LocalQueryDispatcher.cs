using System;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Request.Abstractions;
using Autofac;
using Serilog;

namespace Acnys.Core.Request
{
    public class LocalQueryDispatcher : IDispatchQuery
    {
        private readonly ILifetimeScope _scope;
        private readonly ILogger _log;

        public LocalQueryDispatcher(ILifetimeScope scope, ILogger log)
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
        public async Task<TResult> Dispatch<TResult>(IQuery<TResult> query, CancellationToken cancellationToken)
        {
            var queryName = query.GetType().Name;

            _log.Verbose("Dispatching query {queryName} locally", queryName);

            _log.Verbose("Query object: {@query}", query);

            using (var scope = _scope.BeginLifetimeScope())
            {
                _log.Verbose("Lifetime scope {scopeId} created for query", scope.GetHashCode());

                try
                {
                    var queryType = query.GetType();
                    var requestedHandlerType = typeof(IHandleQuery<,>).MakeGenericType(query.GetType(), typeof(TResult));
                    _log.Verbose("Looking handler with type {handlerType}", requestedHandlerType);

                    var handler = (dynamic) scope.Resolve(requestedHandlerType);
                    Type handlerType = handler.GetType();

                    _log.Verbose("Handling {queryType} locally with {handlerType}", queryType.Name, handlerType.Name);
                    TResult result = await handler.Handle((dynamic) query, cancellationToken);

                    _log.Debug("Local query dispatch completed successfully");
                    _log.Verbose("Query result object: {@queryResult}", result);

                    return result;
                }
                catch (Exception exception)
                {
                    _log.Error(exception, "Local query dispatch failed");
                    throw;
                }
                finally
                {
                    _log.Verbose("Ending lifetime scope {scopeId}", scope.GetHashCode());
                }
            }
        }
    }
}
