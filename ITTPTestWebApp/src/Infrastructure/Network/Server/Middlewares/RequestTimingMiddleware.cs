using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace ITTPTestWebApp.Network
{
    public class RequestTimingMiddleware
    {
        private readonly RequestDelegate _next;
        public RequestTimingMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            DateTime starttime = DateTime.UtcNow;
            var sw = Stopwatch.StartNew();

            context.Response.OnStarting(() =>
            {
                sw.Stop();
                context.Response.Headers["X-Request-Timestamp"] = starttime.ToString("o");
                context.Response.Headers["X-Request-Duration"] = sw.Elapsed.TotalMilliseconds.ToString("F2");
                return Task.CompletedTask;
            });

            await _next(context);
        }
    }
}
