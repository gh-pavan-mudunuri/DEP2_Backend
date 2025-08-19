using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;

namespace backend.Middleware
{
    public class CustomMiddleware
    {
        private readonly RequestDelegate _next;

        public CustomMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Example: Logging request
            Console.WriteLine($"Request: {context.Request.Method} {context.Request.Path}");

            try
            {
                await _next(context); // Call the next middleware
            }
            catch (Exception ex)
            {
                // Example: Global error handling
                Console.WriteLine($"Exception caught in middleware: {ex.Message}");
                context.Response.StatusCode = 500;
                await context.Response.WriteAsJsonAsync(new { success = false, message = "An unexpected error occurred." });
            }

            // Example: Logging response
            Console.WriteLine($"Response: {context.Response.StatusCode}");
        }
    }

    // Extension method for easy registration
    public static class CustomMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CustomMiddleware>();
        }
    }
}
