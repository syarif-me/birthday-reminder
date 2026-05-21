using BirthdayReminder.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BirthdayReminder.API.Infrastructure;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        var statusCode = exception switch
        {
            NotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = exception is NotFoundException ? "Not Found" : "Internal Server Error",
            Detail = exception is NotFoundException ? exception.Message : "An unexpected error occurred.",
            Type = exception is NotFoundException 
                ? "https://tools.ietf.org/html/rfc7231#section-6.5.4" 
                : "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        };

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
