using System.Diagnostics;

namespace ShiftOne.API.Middleware {
    public class RequestLogger {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLogger> _logger;

        public RequestLogger(RequestDelegate next, ILogger<RequestLogger> logger) {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context) {
            var method = context.Request.Method;
            var path = context.Request.Path;
            var stopwatch = Stopwatch.StartNew();
            
            await _next(context);
            
            stopwatch.Stop();

            _logger.LogInformation("{Method} {Path} -> {Status} in {Duration}ms",
                method, path, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
        }
    }
}