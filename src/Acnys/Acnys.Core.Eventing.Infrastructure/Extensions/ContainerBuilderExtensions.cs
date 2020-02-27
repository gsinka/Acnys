using Acnys.Core.Eventing.Abstractions;
using Acnys.Core.Eventing.Infrastructure.Publishers;
using Autofac;

namespace Acnys.Core.Eventing.Infrastructure.Extensions
{
    public static class ContainerBuilderExtensions
    {
        public static ContainerBuilder RegisterLoopbackEventPublisher(this ContainerBuilder builder)
        {
            builder.RegisterType<LoopbackEventPublisher>().As<IPublishEvent>().SingleInstance();
            return builder;
        } 

        public static ContainerBuilder RegisterEventDispatcher(this ContainerBuilder builder)
        {
            builder.RegisterType<EventDispatcher>().As<IDispatchEvent>().SingleInstance();
            return builder;
        } 
        
        public static ContainerBuilder RegisterEventHandlersFromAssemblyOf<T>(this ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(T).Assembly)
                .AsClosedTypesOf(typeof(IHandleEvent<>))
                .AsImplementedInterfaces();

            return builder;
        }

        public static ContainerBuilder RegisterEventAwaiterService(this ContainerBuilder builder)
        {
            builder.RegisterType<EventAwaiterService>().AsSelf().SingleInstance();
            return builder;
        }
    }
}
