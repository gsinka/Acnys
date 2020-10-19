using System;
using System.Collections.Generic;
using System.Net.Http;
using Acnys.Core.Request.Abstractions;
using Acnys.Core.Request.Infrastructure.Senders;
using Autofac;
using Autofac.Features.Indexed;
using Serilog;

namespace Acnys.Core.Request.Infrastructure.Extensions
{
    public static class RequestSenderExtensions
    {
        /// <summary>
        /// Register main request sender
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="keySelector">Key selector to select the request sender to use for the given request</param>
        /// <returns></returns>
        public static ContainerBuilder RegisterRequestSender(this ContainerBuilder builder, Func<IRequest, IDictionary<string, object>, object> keySelector)
        {
            builder.Register(context => new RequestSender(
                context.Resolve<ILogger>().ForContext<RequestSender>(),
                context.Resolve<IIndex<object, ISendRequest>>(),
                keySelector
            )).AsImplementedInterfaces().InstancePerLifetimeScope();

            return builder;
        }

        /// <summary>
        /// Register loopback request sender. Loopback sender sends requests directly to registered dispatchers
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="senderKey"></param>
        /// <returns></returns>
        public static ContainerBuilder RegisterLoopbackRequestSender(this ContainerBuilder builder, object senderKey = null)
        {
            if (senderKey == null)
            {
                builder.RegisterType<LoopbackRequestSender>().AsImplementedInterfaces().InstancePerLifetimeScope();
            }
            else
            {
                builder.RegisterType<LoopbackRequestSender>().Keyed<ISendRequest>(senderKey).InstancePerLifetimeScope();
            }

            return builder;
        }

        /// <summary>
        /// Register HTTP Request sender service
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="uri"></param>
        /// <param name="senderKey"></param>
        /// <returns></returns>
        public static ContainerBuilder RegisterHttpRequestSender(this ContainerBuilder builder, string uri,
            object senderKey)
        {
            if (senderKey == null)
            {
                builder.RegisterType<HttpClientHandler>().AsSelf().SingleInstance();
                builder.Register(context =>
                        new HttpRequestSender(context.Resolve<ILogger>().ForContext<HttpRequestSender>(),
                            context.Resolve<HttpClientHandler>(),
                            uri))
                    .As<ISendRequest>().InstancePerLifetimeScope();
            }
            else
            {
                builder.RegisterType<HttpClientHandler>().Keyed<HttpClientHandler>(senderKey).AsSelf().SingleInstance();
                builder.Register(context =>
                        new HttpRequestSender(context.Resolve<ILogger>().ForContext<HttpRequestSender>(),
                            context.ResolveKeyed<HttpClientHandler>(senderKey),
                            uri))
                    .Keyed<ISendRequest>(senderKey).InstancePerLifetimeScope();
            }

            return builder;
        }
    }
}