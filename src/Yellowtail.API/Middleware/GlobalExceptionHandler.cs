using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Yellowtail.Services.Exceptions;

namespace Yellowtail.API.Middleware;

/// <summary>
/// Translates unhandled exceptions into RFC 7807 <see cref="ProblemDetails"/> responses:
/// <see cref="NotFoundException"/> to 404, <see cref="ValidationFailedException"/> to 400,
/// and anything else to 500.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    /// <summary>
    /// The logger used to record unexpected (500) exceptions.
    /// </summary>
    private readonly ILogger<GlobalExceptionHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalExceptionHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger used to record unexpected (500) exceptions.</param>
    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Handles an exception raised during request processing by writing a <see cref="ProblemDetails"/> response.
    /// </summary>
    /// <param name="httpContext">The context of the request that raised the exception.</param>
    /// <param name="exception">The exception that was raised.</param>
    /// <param name="cancellationToken">A token used to cancel the write operation.</param>
    /// <returns><see langword="true"/>, indicating the exception was handled and no further processing is needed.</returns>
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
            ValidationFailedException => (StatusCodes.Status400BadRequest, "Validation Failed"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception");
        }

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message
        }, cancellationToken);

        return true;
    }
}
