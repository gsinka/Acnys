using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Request.Abstractions;
using Microsoft.Extensions.Logging;

namespace Acnys.Request.Dispatchers
{
    /// <summary>
    /// Query dispatcher
    /// </summary>
    public class QueryDispatcher : IDispatchQuery
    {
        private readonly ILogger _log;
        private readonly IServiceProvider _serviceProvider;

        public QueryDispatcher(ILogger log, IServiceProvider serviceProvider)
        {
            _log = log;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Dispatch query
        /// </summary>
        /// <typeparam name="TResult">Type of query result</typeparam>
        /// <param name="query">Query object</param>
        /// <param name="arguments">Arguments</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public async Task<TResult> DispatchAsync<TResult>(IQuery<TResult> query, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            var queryName = query.GetType().Name;

            _log.LogTrace("Dispatching query {queryName}", queryName);
            _log.LogTrace("Query object: {@query}", query);

            try
            {
                var queryType = query.GetType();
                var requestedHandlerType = typeof(IHandleQuery<,>).MakeGenericType(query.GetType(), typeof(TResult));
                _log.LogTrace("Looking for handler with type {handlerType}", requestedHandlerType);

                var handlers = ((IEnumerable<dynamic>)_serviceProvider.GetService(typeof(IEnumerable<>).MakeGenericType(requestedHandlerType))).ToList();

                if (!handlers.Any())
                {
                    throw new InvalidOperationException($"No query handler registered for query {queryType.FullName}");
                }

                if (handlers.Count() > 1)
                {
                    throw new InvalidOperationException($"Multiple query handlers registered for query {queryType.FullName}. Registered query handlers: {string.Join(",", handlers.Select(x => x.GetType().FullName))}");
                }

                var handler = handlers.Single();
                string handlerType = handler.GetType().Name;

                _log.LogTrace("Handling {queryType} with {handlerType}", queryType.Name, handlerType);
                TResult result = await handler.HandleAsync((dynamic)query, arguments, cancellationToken);

                _log.LogDebug("Query dispatch completed successfully");
                _log.LogTrace("Query result object: {@queryResult}", result);

                return result;
            }
            catch (Exception exception)
            {
                _log.LogError(exception, "Query dispatch failed");
                throw;
            }
        }
    }
}
