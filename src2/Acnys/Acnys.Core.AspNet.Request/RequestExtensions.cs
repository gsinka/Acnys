using System;
using Acnys.Core.Request.Abstractions;
using Acnys.Core.Request.Infrastructure.Extensions;
using Acnys.Core.Request.Infrastructure.Senders;
using Autofac;
using Microsoft.Extensions.Hosting;

namespace Acnys.Core.AspNet.Request
{
    public static class RequestExtensions
    {
        public static IHostBuilder AddRequests(this IHostBuilder builder)
        {
            return builder.ConfigureContainer<ContainerBuilder>((context, containerBuilder) =>
            {
                containerBuilder.RegisterCommandDispatcher();
                containerBuilder.RegisterQueryDispatcher();
            });
        }

        public static IHostBuilder RegisterRequestHandlersFromAssemblyOf<T>(this IHostBuilder builder)
        {
            return builder.ConfigureContainer<ContainerBuilder>((context, containerBuilder) =>
            {
                containerBuilder.RegisterCommandHandlersFromAssemblyOf<T>();
                containerBuilder.RegisterQueryHandlersFromAssemblyOf<T>();
            });
        }

        public static IHostBuilder AddRequestValidation(this IHostBuilder builder)
        {
            return builder.ConfigureContainer<ContainerBuilder>((context, containerBuilder) =>
                {
                    containerBuilder.AddCommandValidationBehaviour();
                    containerBuilder.AddQueryValidationBehaviour();
                });
        }

        public static IHostBuilder RegisterRequestValidatorsFromAssemblyOf<T>(this IHostBuilder builder)
        {
            return builder.ConfigureContainer<ContainerBuilder>((context, containerBuilder) =>
            {
                containerBuilder.RegisterValidatorsFromAssemblyOf<T>();
            });
        }

        public static IHostBuilder AddHttpRequestSender(this IHostBuilder builder, Func<HostBuilderContext, string> uri, object senderKey = null)
        {
            return builder.ConfigureContainer<ContainerBuilder>((context, containerBuilder) =>
                {
                    containerBuilder.RegisterHttpRequestSender(uri(context), senderKey);
                });
        }
        
        public static IHostBuilder AddLoopbackRequestSender(this IHostBuilder builder, object senderKey = null)
        {
            return builder.ConfigureContainer<ContainerBuilder>((context, containerBuilder) =>
                {
                    containerBuilder.RegisterLoopbackRequestSender(senderKey);
                });
        }

        public static IHostBuilder AddRequestSender(this IHostBuilder builder, Func<IRequest, object> keySelector)
        {
            return builder.ConfigureContainer<ContainerBuilder>((context, containerBuilder) =>
            {
                containerBuilder.RegisterRequestSender(keySelector);
            });
        }
    }
}
