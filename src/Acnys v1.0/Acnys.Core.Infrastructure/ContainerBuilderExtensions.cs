﻿using System;
using System.Collections.Generic;
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

        public static ContainerBuilder RegisterEventHandlersFromAssemblyOf<T>(this ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(T).Assembly)
                .AsClosedTypesOf(typeof(IHandleEvent<>))
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
