using Acnys.Core.Request.Abstractions;
using Acnys.Core.Request.Infrastructure.Metrics;
using Acnys.Core.Request.Infrastructure.Validation;
using Autofac;
using System;
using System.Collections.Generic;
using System.Text;

namespace Acnys.Core.Request.Infrastructure.Extensions
{
    public static class MetricsExtensions
    {
        public static ContainerBuilder AddCommandMetricsBehaviour(this ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(CommandMetrics<>));
            builder.RegisterGenericDecorator(typeof(CommandMetrics<>), typeof(IHandleCommand<>));
            return builder;
        }
    }
}
