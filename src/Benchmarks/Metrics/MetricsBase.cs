using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Benchmarks.Metrics
{
    abstract class MetricsBase : IMetrics
    {
        private readonly ISysPerfLogger _sysPerfLogger;
        private readonly MetricsCollection<DateTime> _dates = new MetricsCollection<DateTime>();

        protected abstract string ScenarioName { get; }

        public MetricsBase(ISysPerfLogger sysPerfLogger)
            => _sysPerfLogger = sysPerfLogger;

        public virtual void Reset()
            => _dates.Clear();

        public virtual string GetFormattedMetrics()
        {
            var GCCollectionCount = GetGCCollectionCount();
            return $"Processor usage: {GetProcessorUsage():00.00} %" + Environment.NewLine
                 + $"Gen 0: {GCCollectionCount.Gen0}, Gen 1: {GCCollectionCount.Gen1}, Gen 2: {GCCollectionCount.Gen2}";
        }

        public virtual async Task ExportToCSV(DateTime timestamp)
        {
            using (var stream = File.CreateText(GetPerfMetricsFileName(timestamp)))
            {
                await stream.WriteLineAsync("Processor usage;Gen0;Gen1;Gen2;");
                var GCCollectionCount = GetGCCollectionCount();
                await stream.WriteLineAsync($"{GetProcessorUsage():00.00};{GCCollectionCount.Gen0};{GCCollectionCount.Gen1};{GCCollectionCount.Gen2};");
            }
        }

        protected void AddDate(DateTime date)
            => _dates.Add(date);

        protected double GetMax(IEnumerable<TimeSpan> metrics)
            => metrics.Count() > 0 ? metrics.Max().TotalMilliseconds : 0;

        protected double GetMin(IEnumerable<TimeSpan> metrics)
            => metrics.Count() > 0 ? metrics.Min().TotalMilliseconds : 0;

        protected double GetAverage(IEnumerable<TimeSpan> metrics)
            => metrics.Count() > 0 ? metrics.Average(x => x.TotalMilliseconds) : 0;

        protected double GetMedian(IList<TimeSpan> metrics)
        {
            if (metrics.Count == 0)
                return 0;

            if (metrics.Count % 2 != 0)
                return metrics[metrics.Count / 2].TotalMilliseconds;

            var a = metrics[metrics.Count / 2 - 1].TotalMilliseconds;
            var b = metrics[metrics.Count / 2].TotalMilliseconds;
            return (a + b) / 2;
        }

        protected double GetStdDev(IList<TimeSpan> metrics)
        {
            if (metrics.Count <= 0)
                return 0;

            var avg = metrics.Average(t => t.TotalMilliseconds);
            var sum = metrics.Sum(d => Math.Pow(d.TotalMilliseconds - avg, 2));
            return Math.Sqrt(sum / (metrics.Count - 1));
        }

        protected string GetMetricsFileName(DateTime timestamp)
            => $"metrics - {ScenarioName} - {GetFormattedTimestamp(timestamp)}.csv";

        private string GetPerfMetricsFileName(DateTime timestamp)
            => $"perfMetrics - {ScenarioName} - {GetFormattedTimestamp(timestamp)}.csv";

        private string GetFormattedTimestamp(DateTime date)
            => $"{date.Year:0000}{date.Month:00}{date.Day:00}_{date.Hour:00}{date.Minute:00}{date.Second:00}";

        private double GetProcessorUsage()
        {
            if (_dates.Count == 0)
            {
                return 0;
            }

            var firstMetricDate = _dates.Min();
            var lastMetricDate = _dates.Max();
            var testDuration = lastMetricDate - firstMetricDate;
            var processorUsages = _sysPerfLogger.ProcessorUsages
                .Where(x => x.Key >= firstMetricDate && x.Key <= lastMetricDate)
                .OrderBy(x => x.Key);

            var normalizedProcessorUsage = processorUsages.Last().Value - processorUsages.First().Value;

            return normalizedProcessorUsage.TotalMilliseconds / Environment.ProcessorCount / testDuration.TotalMilliseconds * 100;
        }

        private GCCollectionCount GetGCCollectionCount()
        {
            if (_dates.Count == 0)
            {
                return new GCCollectionCount(0, 0, 0);
            }

            var firstMetricDate = _dates.Min();
            var lastMetricDate = _dates.Max();
            var GCCollectionCounts = _sysPerfLogger.GCCollectionCounts
                .Where(x => x.Key >= firstMetricDate && x.Key <= lastMetricDate)
                .OrderBy(x => x.Key);

            var firstGCCollection = GCCollectionCounts.First().Value;
            var lastGCCollection = GCCollectionCounts.Last().Value;

            return new GCCollectionCount(
                lastGCCollection.Gen0 - firstGCCollection.Gen0,
                lastGCCollection.Gen1 - firstGCCollection.Gen1,
                lastGCCollection.Gen2 - firstGCCollection.Gen2);
        }
    }
}
