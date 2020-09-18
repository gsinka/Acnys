﻿using Prometheus;
using System.Collections.Generic;
using System.Linq;

/*
 Not using quantiles: https://www.robustperception.io/how-does-a-prometheus-summary-work
 */

namespace Acnys.Core.Services
{
    public class MetricsService
    {
        private Summary _summary;
        private Summary Summary
        {
            get
            {
                if (_summary == null)
                    _summary = Metrics.CreateSummary("microservice_summaries", "Auto-generated summary metrics", new SummaryConfiguration() { LabelNames = new[] { "namespace", "name", "metric_type" } });
                return _summary;
            }
        }

        private Counter _counter;

        private Counter Counter
        {
            get
            {
                if (_counter == null) _counter = Metrics.CreateCounter("microservice_counters", "Auto-generated counter metrics", new CounterConfiguration() { LabelNames = new[] { "namespace", "name", "metric_type" } });
                return _counter;
            }
        }
        private Counter _exceptionCounter;
        private Counter ExceptionCounter
        {
            get
            {
                if (_exceptionCounter == null) _exceptionCounter = Metrics.CreateCounter("microservice_exceptions", "Auto-generated exception counter metrics", new CounterConfiguration() { LabelNames = new[] { "file", "line", "exception" } });
                return _exceptionCounter;
            }
        }
        public void ObserveInSummary<T>(double val, string type = "")
        {
            Summary.
                WithLabels(
                typeof(T).Namespace,
                typeof(T).Name,
                type).
                Observe(val);
        }

        public void IncrementCounter<T>(double increment = 1, string type = "")
        {
            Counter.
                WithLabels(
                typeof(T).Namespace,
                typeof(T).Name,
                type).
                Inc(increment);
        }
        public void AddException(string file, string line, string exception)
        {
            ExceptionCounter.
                WithLabels(
                file,
                line,
                exception).
                Inc();
        }
    }
}
