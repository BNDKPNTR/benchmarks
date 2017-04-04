using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Benchmarks.Metrics.RequestLogger
{
    public class RequestLogger : ILogger, IMetrics
    {
        private static readonly Disposable _disposable = new Disposable();
        private readonly MetricsCollection<string> _metrics = new MetricsCollection<string>();

        public IDisposable BeginScope<TState>(TState state)
            => _disposable;

        public bool IsEnabled(LogLevel logLevel)
            => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var msg = formatter(state, exception);

            /* "Request finished in 1.3ms" <- we want to store these type of msg's so check if the 20th char is num.
               Later we will filter more precisely.*/
            if (msg.Length >= 20 && char.IsNumber(msg[20]))
            {
                _metrics.Add(msg);
            }
        }

        public void Reset()
            => _metrics.Clear();

        public IEnumerable<double> GetRequestTimes()
        {
            var result = new List<double>();
            var regex = new System.Text.RegularExpressions.Regex(@"\d+\.?\d*(?=ms)");

            foreach (var metrics in _metrics.Where(s => s.StartsWith("Request finished in ")))
            {
                if (double.TryParse(regex.Match(metrics, 20).Value, out var value))
                {
                    result.Add(value);
                }
            }

            return result;
        }

        public string GetFormattedMetrics()
        {
            var avg = GetRequestTimes().Average();
            return $"Avg: {avg:0.0000} ms";
        }

        public Task ExportToCSV(DateTime timestamp)
        {
            throw new NotImplementedException();
        }

        private class Disposable : IDisposable
        {
            public void Dispose() { }
        }
    }
}
