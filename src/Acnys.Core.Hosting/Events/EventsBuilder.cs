using Acnys.Core.Eventing;
using Acnys.Core.Hosting.Request;
using Acnys.Core.Request.Application;
using Autofac;
using Microsoft.Extensions.Hosting;

namespace Acnys.Core.Hosting.Events
{
    public class EventsBuilder
    {
        protected HostBuilderContext Context { get; }
        protected ContainerBuilder Builder { get; }

        public EventsBuilder(HostBuilderContext context, ContainerBuilder builder)
        {
            Context = context;
            Builder = builder;

            Builder.RegisterType<EventDispatcher>().As<IDispatchEvent>().SingleInstance();
        }

        public EventsBuilder RegisterHandlersFromAssemblyOf<T>()
        {
            Builder.RegisterAssemblyTypes(typeof(T).Assembly)
                .AsClosedTypesOf(typeof(IHandleEvent<>))
                .AsImplementedInterfaces();

            return this;
        }
    }
}