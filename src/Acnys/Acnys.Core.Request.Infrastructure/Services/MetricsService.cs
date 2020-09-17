using Prometheus;
using System.Collections.Generic;
using System.Linq;

/*
 Not using quantiles: https://www.robustperception.io/how-does-a-prometheus-summary-work
 */

namespace Acnys.Core.Request.Infrastructure.Services
{
    public class MetricsService
    {
        private readonly Summary _summary = Prometheus.Metrics.CreateSummary("microservice_summaries", "Auto-generated summary metrics", new SummaryConfiguration() { LabelNames = new[] { "namespace", "name", "type" } });

        private readonly Counter _counter = Prometheus.Metrics.CreateCounter("microservice_counters", "Auto-generated counter metrics", new CounterConfiguration() { LabelNames = new[] { "namespace", "name", "type" } });

        //private readonly List<Counter> _counters = new List<Counter>();
        //private readonly List<Summary> _summaries = new List<Summary>();

        //public Counter GetCounter<T>()
        //{
        //    var name = _counterName + CalculateName(typeof(T).Name);
        //    var metric = _counters.FirstOrDefault(c => c.Name == name);
        //    if (metric == null)
        //    {
        //        metric = Prometheus.Metrics.CreateCounter(name, _counterText + typeof(T).FullName, new CounterConfiguration()
        //        {
        //            LabelNames = new[] { "name" }
        //        });
        //        _counters.Add(metric);
        //    }
        //    return metric;
        //}
        //public Summary GetSummary<T>()
        //{
        //    var name = "";
        //    var metric = _summaries.FirstOrDefault(c => c.Name == name);
        //    if (metric == null)
        //    {
        //        metric = Prometheus.Metrics.CreateSummary(name, _summaryText + typeof(T).FullName,
        //            new SummaryConfiguration()
        //            {
        //                LabelNames = new[] { "name" },
        //            });
        //        _summaries.Add(metric);
        //    }
        //    return metric;
        //}

        // input:   Test2.Almafa.TestCommandFirst
        // output:  test2_almafa__test_command_first
        private string CalculateName(string n)
        {
            var seperated = n.SelectMany((c, i) => i != 0 && char.IsUpper(c) && !char.IsUpper(n[i - 1]) ? new char[] { ' ', c } : new char[] { c });
            return new string(seperated.ToArray()).ToLower().Trim().Replace('.', '_').Replace(' ', '_');
        }

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
