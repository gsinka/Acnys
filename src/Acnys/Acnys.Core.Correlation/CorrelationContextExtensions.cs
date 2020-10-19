using Acnys.Core.Eventing.Abstractions;
using Acnys.Core.Request.Abstractions;
using Autofac;

namespace Acnys.Core.Correlation
{
    public static class CorrelationContextExtensions
    {
        public static ContainerBuilder AddCorrelation(this ContainerBuilder builder)
        {
            builder.RegisterCorrelationContext();

            builder.AddCommandCorrelationBehaviour();
            builder.AddCommandSenderCorrelationBehaviour();
            
            builder.AddQueryCorrelationBehaviour();
            builder.AddQuerySenderCorrelationBehaviour();

            builder.AddEventCorrelationBehaviour();
            builder.AddEventPublisherCorrelationBehaviour();
            
            return builder;
        }

        public static ContainerBuilder RegisterCorrelationContext(this ContainerBuilder builder)
        {
            builder.RegisterType<CorrelationContext>().InstancePerLifetimeScope().AsSelf();
            return builder;
        }

        public static ContainerBuilder AddCommandCorrelationBehaviour(this ContainerBuilder builder)
        {
            builder.RegisterType<CommandCorrelationBehavior>().InstancePerLifetimeScope().AsSelf();
            builder.RegisterDecorator(typeof(CommandCorrelationBehavior), typeof(IDispatchCommand));
            return builder;
        }

        public static ContainerBuilder AddQueryCorrelationBehaviour(this ContainerBuilder builder)
        {
            builder.RegisterType<QueryCorrelationBehavior>().InstancePerLifetimeScope().AsSelf();
            builder.RegisterDecorator(typeof(QueryCorrelationBehavior), typeof(IDispatchQuery));
            return builder;
        }

        public static ContainerBuilder AddEventCorrelationBehaviour(this ContainerBuilder builder)
        {
            builder.RegisterType<EventCorrelationBehavior>().InstancePerLifetimeScope().AsSelf();
            builder.RegisterDecorator(typeof(EventCorrelationBehavior), typeof(IDispatchEvent));
            return builder;
        }

        public static ContainerBuilder AddEventPublisherCorrelationBehaviour(this ContainerBuilder builder)
        {
            builder.RegisterType<EventPublisherCorrelationBehavior>().InstancePerLifetimeScope().AsSelf();
            builder.RegisterDecorator(typeof(EventPublisherCorrelationBehavior), typeof(IPublishEvent));
            return builder;
        }

        public static ContainerBuilder AddCommandSenderCorrelationBehaviour(this ContainerBuilder builder)
        {
            builder.RegisterType<CommandSenderCorrelationBehavior>().InstancePerLifetimeScope().AsSelf();
            builder.RegisterDecorator(typeof(CommandSenderCorrelationBehavior), typeof(ISendCommand));
            return builder;
        }

        public static ContainerBuilder AddQuerySenderCorrelationBehaviour(this ContainerBuilder builder)
        {
            builder.RegisterType<QuerySenderCorrelationBehavior>().InstancePerLifetimeScope().AsSelf();
            builder.RegisterDecorator(typeof(QuerySenderCorrelationBehavior), typeof(ISendQuery));
            return builder;
        }
    }
}