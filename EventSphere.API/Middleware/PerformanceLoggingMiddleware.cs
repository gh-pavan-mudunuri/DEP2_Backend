using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Threading.Tasks;

namespace backend.Middleware
{
    public class PerformanceLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        public PerformanceLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            await _next(context);
            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;
            System.Console.WriteLine($"Request {context.Request.Method} {context.Request.Path} took {elapsedMs} ms");
        }
    }
    public static class PerformanceLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UsePerformanceLoggingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PerformanceLoggingMiddleware>();
        }
    }
}
