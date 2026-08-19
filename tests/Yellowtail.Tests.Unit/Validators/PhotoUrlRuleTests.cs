using Yellowtail.API.Validators;

namespace Yellowtail.Tests.Unit.Validators;

public class PhotoUrlRuleTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IsValidHttpsUrl_NullOrEmpty_ReturnsTrue(string? url)
    {
        Assert.True(PhotoUrlRule.IsValidHttpsUrl(url));
    }

    [Fact]
    public void IsValidHttpsUrl_AbsoluteHttpsUrl_ReturnsTrue()
    {
        Assert.True(PhotoUrlRule.IsValidHttpsUrl("https://res.cloudinary.com/demo/image.jpg"));
    }

    [Fact]
    public void IsValidHttpsUrl_HttpUrl_ReturnsFalse()
    {
        Assert.False(PhotoUrlRule.IsValidHttpsUrl("http://example.com/image.jpg"));
    }

    [Fact]
    public void IsValidHttpsUrl_RelativeUrl_ReturnsFalse()
    {
        Assert.False(PhotoUrlRule.IsValidHttpsUrl("/images/photo.jpg"));
    }

    [Fact]
    public void IsValidHttpsUrl_NotAUrl_ReturnsFalse()
    {
        Assert.False(PhotoUrlRule.IsValidHttpsUrl("not a url"));
    }
}
