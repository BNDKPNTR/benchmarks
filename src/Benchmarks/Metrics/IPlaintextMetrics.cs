using System;

namespace Benchmarks.Metrics
{
    public interface IPlaintextMetrics : IMetrics
    {
        void Add(DateTime date, TimeSpan elapsed);
    }
}
