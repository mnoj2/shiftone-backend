using System.Diagnostics;

namespace ShiftOne.API.Middleware
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var requestId = Guid.NewGuid().ToString();
            var startTime = DateTime.UtcNow;
            var path = context.Request.Path;
            var method = context.Request.Method;

            _logger.LogInformation("Incoming Request: ID={Id}, Time={Time}, Method={Method}, Path={Path}", 
                requestId, startTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), method, path);

            var stopwatch = Stopwatch.StartNew();

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                var statusCode = context.Response.StatusCode;
                var duration = stopwatch.ElapsedMilliseconds;

                _logger.LogInformation("Outgoing Response: ID={Id}, Status={Status}, Duration={Duration}ms", 
                    requestId, statusCode, duration);
            }
        }
    }
}
