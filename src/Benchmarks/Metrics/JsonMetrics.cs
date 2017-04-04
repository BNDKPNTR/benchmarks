using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Benchmarks.Metrics
{
    class JsonMetrics : MetricsBase, IJsonMetrics
    {
        private readonly MetricsCollection<TimeSpan> _metrics = new MetricsCollection<TimeSpan>();

        protected override string ScenarioName => "Json";

        public JsonMetrics(ISysPerfLogger sysPerfLogger) : base(sysPerfLogger)
        {
        }

        public void Add(TimeSpan elapsed, DateTime date)
        {
            base.AddDate(date);
            _metrics.Add(elapsed);
        }

        public override void Reset()
        {
            base.Reset();
            _metrics.Clear();
        }

        public override string GetFormattedMetrics()
        {
            return base.GetFormattedMetrics() + Environment.NewLine
                + Environment.NewLine
                + "Metrics: " + Environment.NewLine
                + $"\tAvg:\t{GetAverage(_metrics):0.0000} ms" + Environment.NewLine
                + $"\tMedian:\t{GetMedian(_metrics.ToList()):0.0000} ms" + Environment.NewLine
                + $"\tStdDev:\t{GetStdDev(_metrics.ToList()):0.0000} ms" + Environment.NewLine
                + $"\tMin:\t{GetMin(_metrics):0.0000} ms" + Environment.NewLine
                + $"\tMax:\t{GetMax(_metrics):0.0000} ms";
        }

        public override async Task ExportToCSV(DateTime timestamp)
        {
            await base.ExportToCSV(timestamp);

            using (var stream = File.CreateText(GetMetricsFileName(timestamp)))
            {
                foreach (var metric in _metrics)
                {
                    await stream.WriteLineAsync($"{metric.TotalMilliseconds};");
                }
            }
        }
    }
}
