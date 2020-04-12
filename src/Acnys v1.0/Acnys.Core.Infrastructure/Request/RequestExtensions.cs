using Acnys.Core.Application.Abstractions;
using Acnys.Core.Infrastructure.Abstractions;
using Autofac;

namespace Acnys.Core.Infrastructure.Request
{
    public static class RequestExtensions
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

    }
}
