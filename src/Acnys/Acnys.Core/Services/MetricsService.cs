using Prometheus;
using System.Collections.Generic;
using System.Linq;

/*
 Not using quantiles: https://www.robustperception.io/how-does-a-prometheus-summary-work
 */

namespace Acnys.Core.Services
{
    public class MetricsService
    {
        private readonly Summary _summary = Metrics.CreateSummary("microservice_summaries", "Auto-generated summary metrics", new SummaryConfiguration() { LabelNames = new[] { "namespace", "method", "metric_type" } });

        private readonly Counter _counter = Metrics.CreateCounter("microservice_counters", "Auto-generated counter metrics", new CounterConfiguration() { LabelNames = new[] { "namespace", "method", "metric_type" } });

        public void ObserveInSummary<T>(double val, string type = "")
        {
            _summary.
                WithLabels(
                typeof(T).Namespace,
                typeof(T).Name,
                type).
                Observe(val);
        }

        public void IncrementCounter<T>(double increment = 1, string type = "")
        {
            _counter.
                WithLabels(
                typeof(T).Namespace,
                typeof(T).Name,
                type).
                Inc(increment);
        }
    }
}
