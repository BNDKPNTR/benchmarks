using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Benchmarks.Metrics
{
    public class SysPerfLogger : ISysPerfLogger
    {
        private ImmutableDictionary<DateTime, TimeSpan> _processorUsages = ImmutableDictionary<DateTime, TimeSpan>.Empty;
        private ImmutableDictionary<DateTime, GCCollectionCount> _GCCollectionCounts = ImmutableDictionary<DateTime, GCCollectionCount>.Empty;

        public TimeSpan LoggingInterval => TimeSpan.FromMilliseconds(200);
        public DateTime ProcessStartTime => Process.GetCurrentProcess().StartTime;
        public IReadOnlyDictionary<DateTime, TimeSpan> ProcessorUsages => _processorUsages;
        public IReadOnlyDictionary<DateTime, GCCollectionCount> GCCollectionCounts => _GCCollectionCounts;

        public SysPerfLogger()
            => StartLogging();

        public void Reset()
        {
            _processorUsages = _processorUsages.Clear();
            _GCCollectionCounts = _GCCollectionCounts.Clear();
        }

        private void StartLogging()
        {
            Task.Run(async () =>
            {
                var process = Process.GetCurrentProcess();

                while (true)
                {
                    var now = DateTime.UtcNow;
                    _processorUsages = _processorUsages.Add(now, process.TotalProcessorTime);
                    _GCCollectionCounts = _GCCollectionCounts.Add(now, new GCCollectionCount(GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2)));
                    await Task.Delay(LoggingInterval);
                }
            });
        }
    }
}
