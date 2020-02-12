using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Policy;
using Acnys.Core.Eventing;
using Acnys.Core.Hosting.Request.Sender;
using Acnys.Core.Request;
using Acnys.Core.Request.Application;
using Acnys.Core.Request.ReadModel;
using Autofac;
using Autofac.Features.Indexed;
using FluentValidation;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Acnys.Core.Hosting.Request
{
    public class RequestBuilder
    {
        //protected HostBuilderContext Context { get; }
        //protected ContainerBuilder Builder { get; }

        //public Dictionary<string, ISendRequest> RequestSenders = new Dictionary<string, ISendRequest>();

        public List<Action<HostBuilderContext, ContainerBuilder>> Callbacks = new List<Action<HostBuilderContext, ContainerBuilder>>();

        public RequestBuilder()
        {
            //Context = context;
            //Builder = builder;

            Callbacks.Add((context, builder) =>
            {
                builder.RegisterType<CommandDispatcher>().As<IDispatchCommand>().SingleInstance();
                builder.RegisterType<QueryDispatcher>().As<IDispatchQuery>().SingleInstance();
            });
        }

        public RequestBuilder RegisterHandlersFromAssemblyOf<T>()
        {
            Callbacks.Add((context, builder) =>
            {
                builder.RegisterAssemblyTypes(typeof(T).Assembly)
                    .AsClosedTypesOf(typeof(IHandleCommand<>))
                    .AsImplementedInterfaces();

                builder.RegisterAssemblyTypes(typeof(T).Assembly)
                    .AsClosedTypesOf(typeof(IHandleQuery<,>))
                    .AsImplementedInterfaces();
            });

            return this;
        }

        public RequestBuilder RegisterValidatorsFromAssemblyOf<T>()
        {
            Callbacks.Add((context, builder) =>
            {
                builder.RegisterAssemblyTypes(typeof(T).Assembly)
                    .AsClosedTypesOf(typeof(IValidator<>))
                    .AsImplementedInterfaces();
            });

            return this;

        }

        public RequestBuilder ValidateRequests()
        {
            Callbacks.Add((context, builder) =>
            {
                builder.RegisterGeneric(typeof(CommandValidator<>));
                builder.RegisterGenericDecorator(typeof(CommandValidator<>), typeof(IHandleCommand<>));

                builder.RegisterGeneric(typeof(QueryValidator<,>));
                builder.RegisterGenericDecorator(typeof(QueryValidator<,>), typeof(IHandleQuery<,>));
            });

            return this;
        }

        public RequestBuilder AddHttpSender(string key, Func<HostBuilderContext, string> uri)
        {
            Callbacks.Add((context, builder) =>
            {
                builder.RegisterType<HttpClientHandler>().AsSelf().SingleInstance();

                builder.Register(componentContext => new HttpRequestSender(
                    componentContext.Resolve<ILogger>().ForContext<HttpRequestSender>(), 
                    componentContext.Resolve<HttpClientHandler>(),
                    uri(context)))
                    .SingleInstance().Keyed<ISendRequest>(key);
            });
            return this;
        }

        public RequestBuilder AddLoopbackSender(object key)
        {
            Callbacks.Add((context, builder) =>
            {
                builder.Register(componentContext => new LoopbackRequestSender( 
                        componentContext.Resolve<ILogger>().ForContext<LoopbackRequestSender>(),
                        componentContext.Resolve<IDispatchCommand>(), 
                        componentContext.Resolve<IDispatchQuery>()))
                    .SingleInstance().Keyed<ISendRequest>(key);
            });
            return this;
        }

        public RequestBuilder RouteRequest(Func<IRequest, object> senderKeySelector)
        {
            Callbacks.Add((context, builder) =>
                {
                    builder.Register(componentContext => new RequestSender(
                            componentContext.Resolve<ILogger>().ForContext<RequestSender>(), 
                            componentContext.Resolve<IIndex<object, ISendRequest>>(), senderKeySelector))
                        .SingleInstance().AsImplementedInterfaces();
                });
            return this;
        }
    }
}