using Acnys.Core.Request.Abstractions;
using Acnys.Core.Request.Infrastructure.Tracing;
using Autofac;

namespace Acnys.Core.Request.Infrastructure.Extensions
{
    public static class TracingExtensions
    {
        public static ContainerBuilder AddCommandTracingBehaviour(this ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(CommandTracingBehaviour<>));
            builder.RegisterGenericDecorator(typeof(CommandTracingBehaviour<>), typeof(IHandleCommand<>));
            return builder;
        }
        public static ContainerBuilder AddRequestTracingBehaviour(this ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(RequestTracingBehaviour<,>));
            builder.RegisterGenericDecorator(typeof(RequestTracingBehaviour<,>), typeof(IHandleQuery<,>));
            return builder;
        }
    }
}
