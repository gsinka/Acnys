using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Acnys.Core.Abstractions;
using Acnys.Core.Application.Abstractions;
using Acnys.Core.Infrastructure.Dispatcher;
using Acnys.Core.Infrastructure.Sender;
using Autofac;
using Autofac.Features.Indexed;
using Serilog;

namespace Acnys.Core.Infrastructure
{
    public static class ContainerBuilderExtensions
    {
        // Handler registrations

        public static ContainerBuilder RegisterCommandHandler<T>(this ContainerBuilder builder)
        {
            if (typeof(T).GetInterfaces().All(type => type.GetGenericTypeDefinition() != typeof(IHandleCommand<>))) 
                throw new InvalidOperationException("The given type cannot be registered as command handler");

            builder.RegisterType<T>().AsImplementedInterfaces().InstancePerLifetimeScope();
            return builder;
        }
        
        public static ContainerBuilder RegisterQueryHandler<T>(this ContainerBuilder builder)
        {
            if (typeof(T).GetInterfaces().All(type => type.GetGenericTypeDefinition() != typeof(IHandleQuery<,>)))
                throw new InvalidOperationException("The given type cannot be registered as query handler");

            builder.RegisterType<T>().AsImplementedInterfaces().InstancePerLifetimeScope();
            return builder;
        }

        public static ContainerBuilder RegisterCommandHandlersFromAssemblyOf<T>(this ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(T).Assembly)
                .AsClosedTypesOf(typeof(IHandleCommand<>))
                .AsImplementedInterfaces();

            return builder;
        }

        public static ContainerBuilder RegisterQueryHandlersFromAssemblyOf<T>(this ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(T).Assembly)
                .AsClosedTypesOf(typeof(IHandleQuery<,>))
                .AsImplementedInterfaces();

            return builder;
        }

        public static ContainerBuilder RegisterEventHandler<T>(this ContainerBuilder builder)
        {
            if (!(
                typeof(IHandleEvent).IsAssignableFrom(typeof(T)) 
                || typeof(T).GetInterfaces().Any(type => type.GetGenericTypeDefinition() == typeof(IHandleEvent<>)))
            )
                throw new InvalidOperationException("The given type cannot be registered as command handler");

            builder.RegisterType<T>().AsImplementedInterfaces().InstancePerLifetimeScope();
            return builder;
        }

        public static ContainerBuilder RegisterEventHandlersFromAssemblyOf<T>(this ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(T).Assembly)
                .AsClosedTypesOf(typeof(IHandleEvent<>))
                .AsImplementedInterfaces();

            builder.RegisterAssemblyTypes(typeof(T).Assembly)
                .Where(type => typeof(IHandleEvent).IsAssignableFrom(type))
                .AsImplementedInterfaces();

            return builder;
        }

        // Dispatcher registrations
        
        public static ContainerBuilder RegisterCommandDispatcher(this ContainerBuilder builder)
        {
            builder.RegisterType<CommandDispatcher>().As<IDispatchCommand>().InstancePerLifetimeScope();
            return builder;
        }

        public static ContainerBuilder RegisterQueryDispatcher(this ContainerBuilder builder)
        {
            builder.RegisterType<QueryDispatcher>().As<IDispatchQuery>().InstancePerLifetimeScope();
            return builder;
        }

        public static ContainerBuilder RegisterEventDispatcher(this ContainerBuilder builder)
        {
            builder.RegisterType<EventDispatcher>().As<IDispatchEvent>().SingleInstance();
            return builder;
        }

        // Sender registrations

        public static ContainerBuilder RegisterCommandBroker(this ContainerBuilder builder, Func<ICommand, IDictionary<string, object>, object> senderKeySelector)
        {
            builder
                .Register(context => new CommandBroker(context.Resolve<ILogger>().ForContext<CommandBroker>(), senderKeySelector, context.Resolve<IIndex<object, ISendCommand>> ()))
                .AsImplementedInterfaces()
                .SingleInstance();
            
            return builder;
        }
        public static ContainerBuilder RegisterQueryBroker(this ContainerBuilder builder, Func<IQuery<dynamic>, IDictionary<string, object>, object> senderKeySelector)
        {
            builder
                .Register(context => new QueryBroker(context.Resolve<ILogger>().ForContext<QueryBroker>(), senderKeySelector, context.Resolve<IIndex<object, ISendQuery>>()))
                .AsImplementedInterfaces()
                .SingleInstance();
            
            return builder;
        }


        public static ContainerBuilder RegisterLoopbackCommandSender(this ContainerBuilder builder, object key = null)
        {
            if (key != null)
            {
                builder.RegisterType<LoopbackCommandSender>().Keyed<ISendCommand>(key).SingleInstance();
            }
            else
            {
                builder.RegisterType<LoopbackCommandSender>().As<ISendCommand>().SingleInstance();
            }
            
            return builder;
        }

        public static ContainerBuilder RegisterHttpCommandSender(this ContainerBuilder builder, string uri, HttpClient httpClient = null, object key = null)
        {
            if (key != null)
            {
                builder.Register((context => new HttpCommandSender(context.Resolve<ILogger>().ForContext<HttpCommandSender>(), uri, httpClient)))
                    .Keyed<ISendCommand>(key).SingleInstance();
            }
            else
            {
                builder.Register((context => new HttpCommandSender(context.Resolve<ILogger>().ForContext<HttpCommandSender>(), uri, httpClient)))
                    .As<ISendCommand>().SingleInstance();
            }
            
            return builder;
        }
        
        public static ContainerBuilder RegisterHttpQuerySender(this ContainerBuilder builder, string uri, HttpClient httpClient = null, object key = null)
        {
            if (key != null)
            {
                builder.Register((context => new HttpQuerySender(context.Resolve<ILogger>().ForContext<HttpQuerySender>(), uri, httpClient)))
                    .Keyed<ISendQuery>(key).SingleInstance();
            }
            else
            {
                builder.Register((context => new HttpQuerySender(context.Resolve<ILogger>().ForContext<HttpQuerySender>(), uri, httpClient)))
                    .As<ISendQuery>().SingleInstance();
            }
            
            return builder;
        }

        public static ContainerBuilder RegisterLoopbackQuerySender(this ContainerBuilder builder, object key = null)
        {
            if (key != null)
            {
                builder.RegisterType<LoopbackQuerySender>().Keyed<ISendQuery>(key).SingleInstance();
            }
            else
            {
                builder.RegisterType<LoopbackQuerySender>().As<ISendQuery>().SingleInstance();
            }

            return builder;
        }

    }
}
