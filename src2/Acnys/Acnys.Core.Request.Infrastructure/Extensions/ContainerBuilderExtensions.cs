﻿using Acnys.Core.Request.Abstractions;
using Acnys.Core.Request.Infrastructure.Dispatchers;
using Acnys.Core.Request.Infrastructure.Senders;
using Autofac;

namespace Acnys.Core.Request.Infrastructure.Extensions
{
    public static class ContainerBuilderExtensions
    {
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
        public static ContainerBuilder RegisterLoopbackRequestSender(this ContainerBuilder builder)
        {
            builder.RegisterType<LoopbackRequestSender>().AsImplementedInterfaces().SingleInstance();
            return builder;
        }


        public static ContainerBuilder RegisterCommandDispatcher(this ContainerBuilder builder)
        {
            builder.RegisterType<CommandDispatcher>().As<IDispatchCommand>().SingleInstance();
            return builder;
        }

        public static ContainerBuilder RegisterQueryDispatcher(this ContainerBuilder builder)
        {
            builder.RegisterType<QueryDispatcher>().As<IDispatchQuery>().SingleInstance();
            return builder;
        }

    }
}