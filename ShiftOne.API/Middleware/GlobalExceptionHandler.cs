using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ShiftOne.API.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An error occurred while processing your request.",
                Detail = exception.Message,
                Type = exception.GetType().Name,
                Instance = httpContext.Request.Path
            };

            if (exception is UnauthorizedAccessException)
            {
                problemDetails.Status = StatusCodes.Status401Unauthorized;
                problemDetails.Title = "Unauthorized access";
            }
            else if (exception is KeyNotFoundException)
            {
                problemDetails.Status = StatusCodes.Status404NotFound;
                problemDetails.Title = "Resource not found";
            }
            else if (exception is ArgumentException || exception is InvalidOperationException)
            {
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Bad request";
            }

            httpContext.Response.StatusCode = problemDetails.Status.Value;

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
