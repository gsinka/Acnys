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
    /// Command dispatcher
    /// </summary>
    public class CommandDispatcher : IDispatchCommand
    {
        private readonly ILogger _log;
        private readonly IServiceProvider _serviceProvider;

        public CommandDispatcher(ILogger<CommandDispatcher> log, IServiceProvider serviceProvider)
        {
            _log = log;
            _serviceProvider = serviceProvider;

            _log.LogTrace("Command dispatcher {dispatcherId} initiated", GetHashCode());
        }

        /// <summary>
        /// Dispatch command to the registered handler
        /// </summary>
        /// <typeparam name="T">Type of command</typeparam>
        /// <param name="command">Command to dispatch</param>
        /// <param name="arguments">Arguments</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public async Task DispatchAsync<T>(T command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default) where T : ICommand
        {
            _log.LogDebug("Dispatching command {commandType}", typeof(T).Name);
            _log.LogTrace("Command object: {@command}", command);
            
            try
            {
                var commandType = typeof(T);
                var handlerType = typeof(IHandleCommand<>).MakeGenericType(commandType);
                
                _log.LogTrace("Looking handler with type {handlerType}", handlerType);
                
                var handlers = ((IEnumerable<IHandleCommand<T>>)_serviceProvider.GetService(typeof(IEnumerable<>).MakeGenericType(handlerType))).ToList();
                
                if (!handlers.Any())
                {
                    throw new InvalidOperationException($"No command handler registered for command {commandType.FullName}");
                }

                if (handlers.Count() > 1)
                {
                    throw new InvalidOperationException($"Multiple command handlers registered for command {commandType.FullName}. Registered command handlers: {string.Join(",", handlers.Select(x => x.GetType().FullName))}");
                }

                var handler = handlers.Single();

                _log.LogTrace("Handling {commandType} with {handler}", commandType.Name, handler.GetType().Name);
                await handler.HandleAsync(command, arguments, cancellationToken);

                _log.LogTrace("Command dispatch completed successfully");
            }
            catch (Exception exception)
            {
                _log.LogTrace(exception, "Command dispatch failed");
                throw;
            }
        }
    }
}
