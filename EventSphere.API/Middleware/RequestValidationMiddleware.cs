using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace backend.Middleware
{
    public class RequestValidationMiddleware
    {
        private readonly RequestDelegate _next;
        public RequestValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            // Add request validation logic here if needed
            await _next(context);
        }
    }
    public static class RequestValidationMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestValidationMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestValidationMiddleware>();
        }
    }
}
