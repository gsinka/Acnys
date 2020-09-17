using Acnys.Core.Eventing.Abstractions;
using Acnys.Core.Eventing.Infrastructure.Metrics;
using Acnys.Core.Services;
using Autofac;

namespace Acnys.Core.Eventing.Infrastructure.Extensions
{
    public static class MetricsExtensions
    {
        public static ContainerBuilder AddMetricsService(this ContainerBuilder builder)
        {
            builder.RegisterType<MetricsService>().SingleInstance().AsSelf();
            return builder;
        }
        public static ContainerBuilder AddEventCountMetricsBehaviour(this ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(EventCountMetricBehaviour<>));
            builder.RegisterGenericDecorator(typeof(EventCountMetricBehaviour<>), typeof(IHandleEvent<>));
            return builder;
        }
        public static ContainerBuilder AddEventDurationMetricsBehaviour(this ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(EventDurationMetricBehaviour<>));
            builder.RegisterGenericDecorator(typeof(EventDurationMetricBehaviour<>), typeof(IHandleEvent<>));
            return builder;
        }
    }
}
