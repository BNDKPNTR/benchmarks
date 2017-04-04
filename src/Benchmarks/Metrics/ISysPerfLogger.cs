using System;
using System.Collections.Generic;

namespace Benchmarks.Metrics
{
    public interface ISysPerfLogger
    {
        TimeSpan LoggingInterval { get; }
        DateTime ProcessStartTime { get; }
        IReadOnlyDictionary<DateTime, TimeSpan> ProcessorUsages { get; }
        IReadOnlyDictionary<DateTime, GCCollectionCount> GCCollectionCounts { get; }
        void Reset();
    }
}
