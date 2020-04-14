using System;
using Acnys.Core.Abstractions;
using Acnys.Core.Eventing.Abstractions;
using Acnys.Core.Eventing.Infrastructure.Publishers;
using Autofac;
using Serilog;

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

        [Obsolete("EventAwaiterService is obsolete and will be removed in version 1.0. Please use IRecordEvent and EventRecorder instead of this.")]
        public static ContainerBuilder RegisterEventAwaiterService(this ContainerBuilder builder)
        {
            builder.RegisterType<EventAwaiterService>().AsSelf().SingleInstance();
            return builder;
        }

        // ReSharper disable once InconsistentNaming

        public static ContainerBuilder RegisterEventRecorderService(this ContainerBuilder builder, int eventTTL = 60000)
        {
            builder
                .Register(context => new EventRecorder(context.Resolve<ILogger>().ForContext<EventRecorder>(), context.Resolve<IClock>(), eventTTL))
                .AsImplementedInterfaces().SingleInstance();
            
            return builder;
        }
    }
}
