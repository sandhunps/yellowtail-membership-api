using Yellowtail.API.Contracts.Members;
using Yellowtail.API.Validators;

namespace Yellowtail.Tests.Unit.Validators;

public class CreateMemberRequestValidatorTests
{
    private readonly CreateMemberRequestValidator _validator = new();

    [Fact]
    public void Validate_ValidRequest_Passes()
    {
        var request = new CreateMemberRequest { FirstName = "Ada", LastName = "Lovelace", Email = "ada@example.com" };
        Assert.True(_validator.Validate(request).IsValid);
    }

    [Fact]
    public void Validate_EmptyFirstName_Fails()
    {
        var request = new CreateMemberRequest { FirstName = "", LastName = "Lovelace", Email = "ada@example.com" };
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateMemberRequest.FirstName));
    }

    [Fact]
    public void Validate_InvalidEmail_Fails()
    {
        var request = new CreateMemberRequest { FirstName = "Ada", LastName = "Lovelace", Email = "not-an-email" };
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateMemberRequest.Email));
    }

    [Fact]
    public void Validate_NonHttpsPhotoUrl_Fails()
    {
        var request = new CreateMemberRequest
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            Email = "ada@example.com",
            PhotoUrl = "http://example.com/a.jpg"
        };
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateMemberRequest.PhotoUrl));
    }

    [Fact]
    public void Validate_NoPhotoUrl_Passes()
    {
        var request = new CreateMemberRequest
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            Email = "ada@example.com",
            PhotoUrl = null
        };
        Assert.True(_validator.Validate(request).IsValid);
    }
}
