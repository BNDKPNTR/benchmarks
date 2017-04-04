using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Benchmarks.Metrics
{
    public interface IJsonMetrics : IMetrics
    {
        void Add(TimeSpan elapsed, DateTime date);
    }
}
