using Microsoft.AspNetCore.Mvc;
using Moq;
using Yellowtail.API.Contracts.Photos;
using Yellowtail.API.Controllers;
using Yellowtail.Services.Contracts;
using Yellowtail.Services.Models;

namespace Yellowtail.Tests.Unit.Controllers;

public class PhotosControllerTests
{
    [Fact]
    public void GetUploadSignature_DefaultExtension_PassesJpgToService()
    {
        var service = new Mock<IPhotoUploadService>();
        service.Setup(s => s.GenerateUploadSignature("jpg")).Returns(new PhotoUploadSignature
        {
            UploadUrl = "https://demo-account.r2.cloudflarestorage.com/demo-bucket/members/abc.jpg?sig=...",
            PublicUrl = "https://pub-demo.r2.dev/members/abc.jpg",
            ContentType = "image/jpeg",
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5)
        });

        var sut = new PhotosController(service.Object);
        var response = sut.GetUploadSignature();

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        var body = Assert.IsType<PhotoUploadSignatureResponse>(ok.Value);
        Assert.Equal("image/jpeg", body.ContentType);
        Assert.Equal("https://pub-demo.r2.dev/members/abc.jpg", body.PublicUrl);
        service.Verify(s => s.GenerateUploadSignature("jpg"), Times.Once);
    }

    [Fact]
    public void GetUploadSignature_ExplicitExtension_PassesItToService()
    {
        var service = new Mock<IPhotoUploadService>();
        service.Setup(s => s.GenerateUploadSignature("png")).Returns(new PhotoUploadSignature
        {
            UploadUrl = "https://demo-account.r2.cloudflarestorage.com/demo-bucket/members/abc.png?sig=...",
            PublicUrl = "https://pub-demo.r2.dev/members/abc.png",
            ContentType = "image/png",
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5)
        });

        var sut = new PhotosController(service.Object);
        sut.GetUploadSignature("png");

        service.Verify(s => s.GenerateUploadSignature("png"), Times.Once);
    }
}
