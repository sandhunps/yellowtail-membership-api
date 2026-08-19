using FluentValidation;
using Yellowtail.API.Contracts;

namespace Yellowtail.API.Validators;

/// <summary>
/// Shared validation rule for member photo URLs.
/// </summary>
public static class PhotoUrlRule
{
    /// <summary>
    /// Checks whether a photo URL is either absent or a well-formed absolute HTTPS URL.
    /// Deliberately provider-agnostic (not tied to any specific CDN's domain), since the
    /// storage provider is expected to change between the POC and production.
    /// </summary>
    /// <param name="photoUrl">The photo URL to validate, or <see langword="null"/>/empty if none was provided.</param>
    /// <returns><see langword="true"/> if the URL is valid or absent; otherwise <see langword="false"/>.</returns>
    public static bool IsValidHttpsUrl(string? photoUrl) =>
        string.IsNullOrEmpty(photoUrl) ||
        (Uri.TryCreate(photoUrl, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps);
}

/// <summary>
/// Validates <see cref="CreateMemberRequest"/> payloads.
/// </summary>
public class CreateMemberRequestValidator : AbstractValidator<CreateMemberRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateMemberRequestValidator"/> class and configures its rules.
    /// </summary>
    public CreateMemberRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Phone).MaximumLength(30);
        RuleFor(x => x.PhotoUrl)
            .Must(PhotoUrlRule.IsValidHttpsUrl)
            .WithMessage("PhotoUrl must be a well-formed absolute HTTPS URL.");
    }
}

/// <summary>
/// Validates <see cref="UpdateMemberRequest"/> payloads.
/// </summary>
public class UpdateMemberRequestValidator : AbstractValidator<UpdateMemberRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateMemberRequestValidator"/> class and configures its rules.
    /// </summary>
    public UpdateMemberRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Phone).MaximumLength(30);
        RuleFor(x => x.PhotoUrl)
            .Must(PhotoUrlRule.IsValidHttpsUrl)
            .WithMessage("PhotoUrl must be a well-formed absolute HTTPS URL.");
    }
}
