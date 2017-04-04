using Microsoft.Extensions.Logging;

namespace Benchmarks.Metrics.RequestLogger
{
    public class RequestLoggerProvider : ILoggerProvider
    {
        private static readonly RequestLogger _logger = new RequestLogger();

        public ILogger CreateLogger(string categoryName)
        {
            return _logger;
        }

        public void Dispose() { }
    }
}
