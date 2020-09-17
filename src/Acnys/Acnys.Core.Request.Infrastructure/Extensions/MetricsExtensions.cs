using Acnys.Core.Request.Abstractions;
using Acnys.Core.Request.Infrastructure.Metrics;
using Acnys.Core.Request.Infrastructure.Services;
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
            builder.RegisterGeneric(typeof(CommandSummaryMetricBehaviour<>));
            builder.RegisterGenericDecorator(typeof(CommandSummaryMetricBehaviour<>), typeof(IHandleCommand<>));
            return builder;
        }
    }
}
