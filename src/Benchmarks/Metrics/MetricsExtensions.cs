using Benchmarks.Configuration;
using Benchmarks.Metrics.RequestLogger;
using Microsoft.Extensions.DependencyInjection;

namespace Benchmarks.Metrics
{
    public static class MetricsExtensions
    {
        public static IServiceCollection AddMetrics(this IServiceCollection services, Scenarios scenarios)
        {
            services.AddSingleton<ISysPerfLogger, SysPerfLogger>();
            services.AddSingleton<RequestLoggerProvider>();

            if (scenarios.Plaintext)
            {
                services.AddSingleton<IPlaintextMetrics, PlaintextMetrics>();
                services.AddSingleton<IMetrics>(sp => sp.GetService<IPlaintextMetrics>());
            }
            else if (scenarios.Json)
            {
                services.AddSingleton<IJsonMetrics, JsonMetrics>();
                services.AddSingleton<IMetrics>(sp => sp.GetService<IJsonMetrics>());
            }
            else if (scenarios.DbSingleQueryEf)
            {
                services.AddSingleton<ISingleQueryEfMetrics, SingleQueryEfMetrics>();
                services.AddSingleton<IMetrics>(sp => sp.GetService<ISingleQueryEfMetrics>());
            }

            return services;
        }
    }
}
