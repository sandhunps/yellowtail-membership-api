using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace Yellowtail.API.Validators;

/// <summary>
/// Extension methods for converting FluentValidation results into ASP.NET Core response types.
/// </summary>
public static class ValidationResultExtensions
{
    /// <summary>
    /// Converts a FluentValidation <see cref="ValidationResult"/> into a <see cref="ValidationProblemDetails"/> response body.
    /// </summary>
    /// <param name="result">The validation result to convert.</param>
    /// <returns>A <see cref="ValidationProblemDetails"/> with errors grouped by property name.</returns>
    public static ValidationProblemDetails ToProblemDetails(this ValidationResult result)
    {
        var errors = result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        return new ValidationProblemDetails(errors);
    }
}
