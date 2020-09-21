using Acnys.Core.Eventing.Abstractions;
using Acnys.Core.Eventing.Infrastructure.Tracing;
using Autofac;

namespace Acnys.Core.Eventing.Infrastructure.Extensions
{
    public static class TracingExtensions
    {
        public static ContainerBuilder AddEventTracingBehaviour(this ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(EventTracingBehaviour<>));
            builder.RegisterGenericDecorator(typeof(EventTracingBehaviour<>), typeof(IHandleEvent<>));
            return builder;
        }
    }
}
