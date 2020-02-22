﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Abstractions.Extensions;
using Acnys.Core.Request.Abstractions;
using Autofac;
using Serilog;
using Serilog.Context;

namespace Acnys.Core.Request.Infrastructure.Dispatchers
{
    /// <summary>
    /// Autofac based command dispatcher
    /// </summary>
    public class CommandDispatcher : IDispatchCommand
    {
        private readonly ILifetimeScope _scope;
        private readonly ILogger _log;

        public CommandDispatcher(ILifetimeScope scope, ILogger log)
        {
            _scope = scope;
            _log = log;
        }

        /// <summary>
        /// Dispatch command to the registered handler
        /// </summary>
        /// <typeparam name="T">Type of command</typeparam>
        /// <param name="command">Command to dispatch</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public Task Dispatch<T>(T command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default) where T : ICommand
        {
            return Task.Run(async() =>
            {
                //using var correlationId = LogContext.PushProperty("correlationId", arguments.CorrelationId());
                //using var causationId = LogContext.PushProperty("causationId", arguments.CausationId());

                _log.Debug("Dispatching command {commandType}", typeof(T).Name);

                _log.Verbose("Command object: {@command}", command);

                using var scope = _scope.BeginLifetimeScope();

                _log.Verbose("Lifetime scope {scopeId} created for command", scope.GetHashCode());

                try
                {
                    var commandType = typeof(T);
                    var handlerType = typeof(IHandleCommand<>).MakeGenericType(commandType);
                    _log.Verbose("Looking handler with type {handlerType}", handlerType);

                    var handler = (IHandleCommand<T>)scope.Resolve(handlerType);
                            
                    _log.Verbose("Handling {commandType} with {handler}", commandType.Name, handler.GetType().Name);
                    await handler.Handle(command, arguments, cancellationToken);

                    _log.Debug("Command dispatch completed successfully");
                }
                catch (Exception exception)
                {
                    _log.Error(exception, "Command dispatch failed");
                    throw;
                }
                finally
                {
                    _log.Verbose("Ending lifetime scope {scopeId}", scope.GetHashCode());
                }
            }, cancellationToken);
        }
    }
}
