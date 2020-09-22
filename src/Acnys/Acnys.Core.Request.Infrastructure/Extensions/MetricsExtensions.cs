using Acnys.Core.Request.Abstractions;
using Acnys.Core.Request.Infrastructure.Metrics;
using Acnys.Core.Services;
using Autofac;

namespace Acnys.Core.Request.Infrastructure.Extensions
{
    public static class MetricsExtensions
    {
        public static ContainerBuilder AddMetricsService(this ContainerBuilder builder)
        {
            builder.RegisterType<MetricsService>().SingleInstance().AsSelf();
            return builder;
        }
        public static ContainerBuilder AddCommandCountMetricsBehaviour(this ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(CommandCountMetricBehaviour<>));
            builder.RegisterGenericDecorator(typeof(CommandCountMetricBehaviour<>), typeof(IHandleCommand<>));
            return builder;
        }
        public static ContainerBuilder AddCommandDurationMetricsBehaviour(this ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(CommandDurationMetricBehaviour<>));
            builder.RegisterGenericDecorator(typeof(CommandDurationMetricBehaviour<>), typeof(IHandleCommand<>));
            return builder;
        }
        public static ContainerBuilder AddQueryDurationMetrics(this ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(RequestDurationMetricBehaviour<,>));
            builder.RegisterGenericDecorator(typeof(RequestDurationMetricBehaviour<,>), typeof(IHandleQuery<,>));
            return builder;
        }
    }
}
