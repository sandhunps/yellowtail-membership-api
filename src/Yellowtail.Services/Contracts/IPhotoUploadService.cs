using Yellowtail.Services.Models;

namespace Yellowtail.Services.Contracts;

/// <summary>
/// Provides pre-signed URLs for direct-to-R2 photo uploads, so the backend never has to
/// handle image bytes itself.
/// </summary>
public interface IPhotoUploadService
{
    /// <summary>
    /// Generates a short-lived, pre-signed URL a client can use to upload one image of the
    /// given type directly to R2.
    /// </summary>
    /// <param name="fileExtension">The image file extension (without a leading dot) — "jpg", "jpeg", "png", or "webp".</param>
    /// <returns>The pre-signed upload URL and the resulting public URL.</returns>
    /// <exception cref="Exceptions.ValidationFailedException"><paramref name="fileExtension"/> is not a supported image type.</exception>
    PhotoUploadSignature GenerateUploadSignature(string fileExtension);
}
