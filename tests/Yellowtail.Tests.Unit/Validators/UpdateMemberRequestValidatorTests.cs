using Yellowtail.API.Contracts.Members;
using Yellowtail.API.Validators;

namespace Yellowtail.Tests.Unit.Validators;

public class UpdateMemberRequestValidatorTests
{
    private readonly UpdateMemberRequestValidator _validator = new();

    [Fact]
    public void Validate_ValidRequest_Passes()
    {
        var request = new UpdateMemberRequest { FirstName = "Ada", LastName = "Lovelace", Email = "ada@example.com" };
        Assert.True(_validator.Validate(request).IsValid);
    }

    [Fact]
    public void Validate_EmptyLastName_Fails()
    {
        var request = new UpdateMemberRequest { FirstName = "Ada", LastName = "", Email = "ada@example.com" };
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateMemberRequest.LastName));
    }

    [Fact]
    public void Validate_NonHttpsPhotoUrl_Fails()
    {
        var request = new UpdateMemberRequest
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            Email = "ada@example.com",
            PhotoUrl = "ftp://example.com/a.jpg"
        };
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateMemberRequest.PhotoUrl));
    }
}
