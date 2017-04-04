using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Benchmarks.Metrics
{
    public interface ISingleQueryEfMetrics : IMetrics
    {
        void Add(TimeSpan total, TimeSpan dbLoading, TimeSpan serializing, TimeSpan writing, DateTime date);
    }
}
