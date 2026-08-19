using FluentValidation.Results;
using Yellowtail.API.Validators;

namespace Yellowtail.Tests.Unit.Validators;

public class ValidationResultExtensionsTests
{
    [Fact]
    public void ToProblemDetails_GroupsErrorsByPropertyName()
    {
        var result = new ValidationResult(new[]
        {
            new ValidationFailure("FirstName", "First name is required."),
            new ValidationFailure("FirstName", "First name is too long."),
            new ValidationFailure("Email", "Email is invalid.")
        });

        var problemDetails = result.ToProblemDetails();

        Assert.Equal(2, problemDetails.Errors.Count);
        Assert.Equal(2, problemDetails.Errors["FirstName"].Length);
        Assert.Single(problemDetails.Errors["Email"]);
        Assert.Contains("First name is required.", problemDetails.Errors["FirstName"]);
    }

    [Fact]
    public void ToProblemDetails_NoErrors_ReturnsEmptyErrorsDictionary()
    {
        var result = new ValidationResult();
        var problemDetails = result.ToProblemDetails();
        Assert.Empty(problemDetails.Errors);
    }
}
