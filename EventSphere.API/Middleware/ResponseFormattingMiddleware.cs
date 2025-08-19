using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace backend.Middleware
{
    public class ResponseFormattingMiddleware
    {
        private readonly RequestDelegate _next;
        public ResponseFormattingMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);
            // Add response formatting logic here if needed
        }
    }
    public static class ResponseFormattingMiddlewareExtensions
    {
        public static IApplicationBuilder UseResponseFormattingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ResponseFormattingMiddleware>();
        }
    }
}
