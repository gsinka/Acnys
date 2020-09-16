using System;
using System.Collections.Generic;
using Acnys.Core.Request.Abstractions;
using Acnys.Core.Request.Infrastructure.Extensions;
using Autofac;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Acnys.Core.AspNet.Request
{
    public static class RequestExtensions
    {
        public static IHostBuilder AddRequests(this IHostBuilder builder)
        {
            return builder.ConfigureContainer<ContainerBuilder>((context, containerBuilder) =>
            {
                Log.Verbose("Registering command dispatcher");
                containerBuilder.RegisterCommandDispatcher();

                Log.Verbose("Registering query dispatcher");
                containerBuilder.RegisterQueryDispatcher();
            });
        }

        public static IHostBuilder RegisterRequestHandlersFromAssemblyOf<T>(this IHostBuilder builder)
        {
            return builder.ConfigureContainer<ContainerBuilder>((context, containerBuilder) =>
            {
                Log.Verbose("Registering command handlers from assembly {assembly}", typeof(T).Assembly.FullName);
                containerBuilder.RegisterCommandHandlersFromAssemblyOf<T>();

                Log.Verbose("Registering query handlers from assembly {assembly}", typeof(T).Assembly.FullName);
                containerBuilder.RegisterQueryHandlersFromAssemblyOf<T>();
            });
        }

        public static IHostBuilder AddRequestMetrics(this IHostBuilder builder)
        {
            return builder.ConfigureContainer<ContainerBuilder>((context, containerBuilder) =>
            {
                Log.Verbose("Adding request Matrics");
                containerBuilder.AddCommandMetricsBehaviour();
            });
        }

        public static IHostBuilder AddRequestValidation(this IHostBuilder builder)
        {
            return builder.ConfigureContainer<ContainerBuilder>((context, containerBuilder) =>
                {
                    Log.Verbose("Adding command validation behaviour");
                    containerBuilder.AddCommandValidationBehaviour();

                    Log.Verbose("Adding query validation behaviour");
                    containerBuilder.AddQueryValidationBehaviour();
                });
        }

        public static IHostBuilder RegisterRequestValidatorsFromAssemblyOf<T>(this IHostBuilder builder)
        {
            return builder.ConfigureContainer<ContainerBuilder>((context, containerBuilder) =>
            {
                Log.Verbose("Registering validators from assembly {assembly}", typeof(T).Assembly.FullName);
                containerBuilder.RegisterValidatorsFromAssemblyOf<T>();
            });
        }

        public static IHostBuilder AddHttpRequestSender(this IHostBuilder builder, Func<HostBuilderContext, string> uri, object senderKey = null)
        {
            return builder.ConfigureContainer<ContainerBuilder>((context, containerBuilder) =>
            {
                var senderUri = uri(context);
                Log.Verbose("Registering HTTP request sender with key {senderKey} for URI {uri}", senderKey, senderUri);
                containerBuilder.RegisterHttpRequestSender(senderUri, senderKey);
            });
        }

        public static IHostBuilder AddLoopbackRequestSender(this IHostBuilder builder, object senderKey = null)
        {
            return builder.ConfigureContainer<ContainerBuilder>((context, containerBuilder) =>
                {
                    Log.Verbose("Registering Loopback request sender with key {senderKey}", senderKey);
                    containerBuilder.RegisterLoopbackRequestSender(senderKey);
                });
        }

        public static IHostBuilder AddRequestSender(this IHostBuilder builder, Func<IRequest, object> keySelector)
        {
            return AddRequestSender(builder, (request, arguments) => keySelector(request));
        }
        
        public static IHostBuilder AddRequestSender(this IHostBuilder builder, Func<IRequest, IDictionary<string, object>, object> keySelector)
        {
            return builder.ConfigureContainer<ContainerBuilder>((context, containerBuilder) =>
            {
                Log.Verbose("Adding request sender service");
                containerBuilder.RegisterRequestSender(keySelector);
            });
        }
    }
}
