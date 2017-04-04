using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Benchmarks.Metrics
{
    public interface IMetrics
    {
        void Reset();
        string GetFormattedMetrics();
        Task ExportToCSV(DateTime timestamp);
    }
}
