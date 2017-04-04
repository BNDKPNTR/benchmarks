// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information. 

using Benchmarks.Configuration;
using Benchmarks.Data;
using Benchmarks.Metrics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Benchmarks.Middleware
{
    public class SingleQueryEfMiddleware
    {
        private static readonly PathString _path = new PathString(Scenarios.GetPath(s => s.DbSingleQueryEf));
        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private readonly RequestDelegate _next;
        private readonly ISingleQueryEfMetrics _metrics;

        public SingleQueryEfMiddleware(RequestDelegate next, ISingleQueryEfMetrics metrics)
        {
            _next = next;
            _metrics = metrics;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Request.Path.StartsWithSegments(_path, StringComparison.Ordinal))
            {
                var total = Stopwatch.StartNew();
                var db = httpContext.RequestServices.GetService<EfDb>();

                var dbLoading = Stopwatch.StartNew();
                var row = await db.LoadSingleQueryRow();
                dbLoading.Stop();

                var serializing = Stopwatch.StartNew();
                var result = JsonConvert.SerializeObject(row, _jsonSettings);
                serializing.Stop();

                httpContext.Response.StatusCode = StatusCodes.Status200OK;
                httpContext.Response.ContentType = "application/json";
                httpContext.Response.ContentLength = result.Length;

                var writing = Stopwatch.StartNew();
                await httpContext.Response.WriteAsync(result);
                writing.Stop();

                total.Stop();

                _metrics.Add(total.Elapsed, dbLoading.Elapsed, serializing.Elapsed, writing.Elapsed, DateTime.UtcNow);

                return;
            }

            await _next(httpContext);
        }
    }

    public static class SingleQueryEfMiddlewareExtensions
    {
        public static IApplicationBuilder UseSingleQueryEf(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SingleQueryEfMiddleware>();
        }
    }
}
