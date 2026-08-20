using Microsoft.AspNetCore.Mvc;
using Yellowtail.API.Contracts;
using Yellowtail.Services.Contracts;

namespace Yellowtail.API.Controllers;

/// <summary>
/// Issues pre-signed URLs for uploading member photos directly to Cloudflare R2.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PhotosController : ControllerBase
{
    /// <summary>
    /// The service used to generate pre-signed upload URLs.
    /// </summary>
    private readonly IPhotoUploadService _photoUploadService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PhotosController"/> class.
    /// </summary>
    /// <param name="photoUploadService">The service used to generate pre-signed upload URLs.</param>
    public PhotosController(IPhotoUploadService photoUploadService)
    {
        _photoUploadService = photoUploadService;
    }

    /// <summary>
    /// Gets a short-lived, pre-signed URL for uploading one image directly to R2. The client
    /// PUTs the file straight to R2 using <c>uploadUrl</c>, then submits <c>publicUrl</c> as
    /// <c>photoUrl</c> on a member create/update request.
    /// </summary>
    /// <param name="extension">The image file extension: "jpg", "jpeg", "png", or "webp". Defaults to "jpg".</param>
    /// <returns>The pre-signed upload URL and the resulting public URL.</returns>
    /// <exception cref="Yellowtail.Services.Exceptions.ValidationFailedException"><paramref name="extension"/> is not a supported image type.</exception>
    [HttpGet("upload-signature")]
    public ActionResult<PhotoUploadSignatureResponse> GetUploadSignature([FromQuery] string extension = "jpg")
    {
        var signature = _photoUploadService.GenerateUploadSignature(extension);
        return Ok(PhotoUploadSignatureResponse.FromModel(signature));
    }
}
