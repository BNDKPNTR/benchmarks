using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Benchmarks.Metrics
{
    class SingleQueryEfMetrics : MetricsBase, ISingleQueryEfMetrics
    {
        private readonly MetricsCollection<TimeSpan> _totals = new MetricsCollection<TimeSpan>();
        private readonly MetricsCollection<TimeSpan> _dbLoadings = new MetricsCollection<TimeSpan>();
        private readonly MetricsCollection<TimeSpan> _serializings = new MetricsCollection<TimeSpan>();
        private readonly MetricsCollection<TimeSpan> _writings = new MetricsCollection<TimeSpan>();

        protected override string ScenarioName => "SingleQueryEf";

        public SingleQueryEfMetrics(ISysPerfLogger sysPerfLogger) : base(sysPerfLogger)
        {
        }

        public void Add(TimeSpan total, TimeSpan dbLoading, TimeSpan serializing, TimeSpan writing, DateTime date)
        {
            base.AddDate(date);
            _totals.Add(total);
            _dbLoadings.Add(dbLoading);
            _serializings.Add(serializing);
            _writings.Add(writing);
        }

        public override void Reset()
        {
            base.Reset();
            _totals.Clear();
            _dbLoadings.Clear();
            _serializings.Clear();
            _writings.Clear();
        }

        public override string GetFormattedMetrics()
        {
            var totals = "\tTotal" + Environment.NewLine
                      + $"\t\tAvg:\t{GetAverage(_totals):0.0000} ms" + Environment.NewLine
                      + $"\t\tMedian:\t{GetMedian(_totals.ToList()):0.0000} ms" + Environment.NewLine
                      + $"\t\tStdDev:\t{GetStdDev(_totals.ToList()):0.0000} ms" + Environment.NewLine
                      + $"\t\tMin:\t{GetMin(_totals):0.0000} ms" + Environment.NewLine
                      + $"\t\tMax:\t{GetMax(_totals):0.0000} ms";

            var dbLoadings = "\tDb Loading" + Environment.NewLine
                          + $"\t\tAvg:\t{GetAverage(_dbLoadings):0.0000} ms" + Environment.NewLine
                          + $"\t\tMedian:\t{GetMedian(_dbLoadings.ToList()):0.0000} ms" + Environment.NewLine
                          + $"\t\tStdDev:\t{GetStdDev(_dbLoadings.ToList()):0.0000} ms" + Environment.NewLine
                          + $"\t\tMin:\t{GetMin(_dbLoadings):0.0000} ms" + Environment.NewLine
                          + $"\t\tMax:\t{GetMax(_dbLoadings):0.0000} ms";

            var serializings = "\tSerializing" + Environment.NewLine
                            + $"\t\tAvg:\t{GetAverage(_serializings):0.0000} ms" + Environment.NewLine
                            + $"\t\tMedian:\t{GetMedian(_serializings.ToList()):0.0000} ms" + Environment.NewLine
                            + $"\t\tStdDev:\t{GetStdDev(_serializings.ToList()):0.0000} ms" + Environment.NewLine
                            + $"\t\tMin:\t{GetMin(_serializings):0.0000} ms" + Environment.NewLine
                            + $"\t\tMax:\t{GetMax(_serializings):0.0000} ms";

            var writings = "\tWriting to HttpContext" + Environment.NewLine
                        + $"\t\tAvg:\t{GetAverage(_writings):0.0000} ms" + Environment.NewLine
                        + $"\t\tMedian:\t{GetMedian(_writings.ToList()):0.0000} ms" + Environment.NewLine
                        + $"\t\tStdDev:\t{GetStdDev(_writings.ToList()):0.0000} ms" + Environment.NewLine
                        + $"\t\tMin:\t{GetMin(_writings):0.0000} ms" + Environment.NewLine
                        + $"\t\tMax:\t{GetMax(_writings):0.0000} ms";

            return base.GetFormattedMetrics() + Environment.NewLine
                + Environment.NewLine
                + "Metrics: " + Environment.NewLine
                + totals + Environment.NewLine
                + Environment.NewLine
                + dbLoadings + Environment.NewLine
                + Environment.NewLine
                + serializings + Environment.NewLine
                + Environment.NewLine
                + writings + Environment.NewLine
                + Environment.NewLine;
        }

        public override async Task ExportToCSV(DateTime timestamp)
        {
            await base.ExportToCSV(timestamp);

            using (var stream = File.CreateText(GetMetricsFileName(timestamp)))
            {
                foreach (var metric in GetMetricsCombined())
                {
                    await stream.WriteLineAsync($"{metric.total};{metric.dbLoading};{metric.serializing};{metric.writing};");
                }
            }

            IEnumerable<(double total, double dbLoading, double serializing, double writing)> GetMetricsCombined()
            {
                var dbLoadingsEnumerator = _dbLoadings.GetEnumerator();
                var serializingsEnumerator = _serializings.GetEnumerator();
                var writingsEnumerator = _writings.GetEnumerator();

                foreach (var total in _totals)
                {
                    dbLoadingsEnumerator.MoveNext();
                    serializingsEnumerator.MoveNext();
                    writingsEnumerator.MoveNext();

                    yield return (total.TotalMilliseconds,
                        dbLoadingsEnumerator.Current.TotalMilliseconds,
                        serializingsEnumerator.Current.TotalMilliseconds,
                        writingsEnumerator.Current.TotalMilliseconds);
                }
            }
        }
    }
}
