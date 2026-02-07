using BillScanner.Controllers.Base;
using EntityFramework.Exceptions.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BillScanner.Middleware
{
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            logger.LogError(exception, "An exception occurred: {Message}", exception.Message);

            ErrorResponse errorResponse;
            int statusCode;

            switch (exception)
            {
                // Authentication & Authorization Exceptions
                case UnauthorizedAccessException unauthorizedEx:
                    statusCode = StatusCodes.Status401Unauthorized;
                    errorResponse = new ErrorResponse
                    {
                        Message = unauthorizedEx.Message,
                        Details = "Authentication failed or access denied"
                    };
                    logger.LogWarning("Unauthorized access: {Message}", unauthorizedEx.Message);
                    break;

                // Business Logic Exceptions
                case InvalidOperationException invalidOpEx:
                    statusCode = StatusCodes.Status400BadRequest;
                    errorResponse = new ErrorResponse
                    {
                        Message = invalidOpEx.Message,
                        Details = "Invalid operation"
                    };
                    logger.LogWarning("Invalid operation: {Message}", invalidOpEx.Message);
                    break;

                case ArgumentException argEx:
                    statusCode = StatusCodes.Status400BadRequest;
                    errorResponse = new ErrorResponse
                    {
                        Message = argEx.Message,
                        Details = "Invalid argument provided"
                    };
                    logger.LogWarning("Invalid argument: {Message}", argEx.Message);
                    break;

                // Database Exceptions (from EF Core Interceptor)
                case UniqueConstraintException uniqueEx:
                    statusCode = StatusCodes.Status409Conflict;
                    errorResponse = new ErrorResponse
                    {
                        Message = "A record with this information already exists",
                        Details = uniqueEx.Message
                    };
                    logger.LogWarning("Unique constraint violation: {Message}", uniqueEx.Message);
                    break;

                case ReferenceConstraintException refEx:
                    statusCode = StatusCodes.Status409Conflict;
                    errorResponse = new ErrorResponse
                    {
                        Message = "Cannot perform this operation due to related data",
                        Details = refEx.Message
                    };
                    logger.LogWarning("Foreign key constraint violation: {Message}", refEx.Message);
                    break;

                case CannotInsertNullException nullEx:
                    statusCode = StatusCodes.Status400BadRequest;
                    errorResponse = new ErrorResponse
                    {
                        Message = "Required field is missing",
                        Details = nullEx.Message
                    };
                    logger.LogWarning("Null constraint violation: {Message}", nullEx.Message);
                    break;

                case MaxLengthExceededException maxLengthEx:
                    statusCode = StatusCodes.Status400BadRequest;
                    errorResponse = new ErrorResponse
                    {
                        Message = "Input exceeds maximum allowed length",
                        Details = maxLengthEx.Message
                    };
                    logger.LogWarning("Max length exceeded: {Message}", maxLengthEx.Message);
                    break;

                case NumericOverflowException numericEx:
                    statusCode = StatusCodes.Status400BadRequest;
                    errorResponse = new ErrorResponse
                    {
                        Message = "Numeric value is out of range",
                        Details = numericEx.Message
                    };
                    logger.LogWarning("Numeric overflow: {Message}", numericEx.Message);
                    break;

                // KeyNotFoundException
                case KeyNotFoundException notFoundEx:
                    statusCode = StatusCodes.Status404NotFound;
                    errorResponse = new ErrorResponse
                    {
                        Message = "The requested resource was not found",
                        Details = notFoundEx.Message
                    };
                    logger.LogWarning("Resource not found: {Message}", notFoundEx.Message);
                    break;

                // Task Cancelled / Timeout
                case TaskCanceledException or OperationCanceledException:
                    statusCode = StatusCodes.Status408RequestTimeout;
                    errorResponse = new ErrorResponse
                    {
                        Message = "The request was cancelled or timed out",
                        Details = exception.Message
                    };
                    logger.LogWarning("Request cancelled: {Message}", exception.Message);
                    break;

                // Default: Internal Server Error
                default:
                    statusCode = StatusCodes.Status500InternalServerError;
                    errorResponse = new ErrorResponse
                    {
                        Message = "An unexpected error occurred",
                        Details = httpContext.RequestServices
                            .GetRequiredService<IHostEnvironment>()
                            .IsDevelopment()
                                || httpContext.RequestServices.GetRequiredService<IHostEnvironment>().IsEnvironment("Test")
                            ? exception.Message
                            : "Please contact support if the problem persists"
                    };
                    logger.LogError(exception, "Unhandled exception occurred");
                    break;
            }

            // Write standardized error response
            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/json";

            await httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);

            return true; // Exception handled
        }
    }
}