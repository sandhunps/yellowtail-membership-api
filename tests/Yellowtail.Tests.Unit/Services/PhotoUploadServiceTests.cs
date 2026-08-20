using Microsoft.Extensions.Options;
using Yellowtail.Services.Configuration;
using Yellowtail.Services.Exceptions;
using Yellowtail.Services.Implementation;

namespace Yellowtail.Tests.Unit.Services;

public class PhotoUploadServiceTests
{
    private static PhotoUploadService CreateSut(string uploadFolder = "members") =>
        new(Options.Create(new R2Options
        {
            AccountId = "demo-account",
            BucketName = "demo-bucket",
            PublicBaseUrl = "https://pub-demo.r2.dev",
            AccessKeyId = "demo-key",
            SecretAccessKey = "demo-secret",
            UploadFolder = uploadFolder
        }));

    [Theory]
    [InlineData("jpg", "image/jpeg")]
    [InlineData("jpeg", "image/jpeg")]
    [InlineData("png", "image/png")]
    [InlineData("webp", "image/webp")]
    [InlineData("JPG", "image/jpeg")]
    public void GenerateUploadSignature_SupportedExtension_ReturnsMatchingContentType(string extension, string expectedContentType)
    {
        var signature = CreateSut().GenerateUploadSignature(extension);

        Assert.Equal(expectedContentType, signature.ContentType);
    }

    [Fact]
    public void GenerateUploadSignature_UnsupportedExtension_ThrowsValidationFailedException()
    {
        Assert.Throws<ValidationFailedException>(() => CreateSut().GenerateUploadSignature("gif"));
    }

    [Fact]
    public void GenerateUploadSignature_ReturnsNonEmptyUploadUrl()
    {
        var signature = CreateSut().GenerateUploadSignature("jpg");

        Assert.False(string.IsNullOrWhiteSpace(signature.UploadUrl));
        Assert.StartsWith("https://demo-account.r2.cloudflarestorage.com/", signature.UploadUrl);
    }

    [Fact]
    public void GenerateUploadSignature_PublicUrl_UsesConfiguredBaseUrlAndFolder()
    {
        var signature = CreateSut(uploadFolder: "members").GenerateUploadSignature("png");

        Assert.StartsWith("https://pub-demo.r2.dev/members/", signature.PublicUrl);
        Assert.EndsWith(".png", signature.PublicUrl);
    }

    [Fact]
    public void GenerateUploadSignature_NeverIncludesSecretAccessKey()
    {
        var signature = CreateSut().GenerateUploadSignature("jpg");

        Assert.DoesNotContain("demo-secret", signature.UploadUrl);
        Assert.DoesNotContain("demo-secret", signature.PublicUrl);
    }

    [Fact]
    public void GenerateUploadSignature_ExpiresAtIsAFewMinutesInTheFuture()
    {
        var before = DateTimeOffset.UtcNow;
        var signature = CreateSut().GenerateUploadSignature("jpg");
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(signature.ExpiresAt, before.AddMinutes(4), after.AddMinutes(6));
    }

    [Fact]
    public void GenerateUploadSignature_EachCall_GeneratesUniqueObjectKey()
    {
        var sut = CreateSut();

        var first = sut.GenerateUploadSignature("jpg");
        var second = sut.GenerateUploadSignature("jpg");

        Assert.NotEqual(first.PublicUrl, second.PublicUrl);
    }
}
