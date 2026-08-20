using Yellowtail.Services.Models;

namespace Yellowtail.API.Contracts;

/// <summary>
/// Represents a pre-signed URL a client uses to upload one image directly to R2, without
/// routing the file through this API.
/// </summary>
public class PhotoUploadSignatureResponse
{
    /// <summary>
    /// The URL the client must send a single HTTP PUT to, with the raw image bytes as the
    /// request body and <see cref="ContentType"/> set as the Content-Type header.
    /// </summary>
    public string UploadUrl { get; set; } = string.Empty;

    /// <summary>
    /// The URL the uploaded image will be readable at afterward. Use this as the member's
    /// <c>photoUrl</c> once the upload succeeds.
    /// </summary>
    public string PublicUrl { get; set; } = string.Empty;

    /// <summary>
    /// The exact Content-Type header the client's PUT request must send.
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// When the upload URL stops being valid.
    /// </summary>
    public DateTimeOffset ExpiresAt { get; set; }

    /// <summary>
    /// Creates a <see cref="PhotoUploadSignatureResponse"/> from a <see cref="PhotoUploadSignature"/>.
    /// </summary>
    /// <param name="signature">The source signature.</param>
    /// <returns>The mapped response.</returns>
    public static PhotoUploadSignatureResponse FromModel(PhotoUploadSignature signature) => new()
    {
        UploadUrl = signature.UploadUrl,
        PublicUrl = signature.PublicUrl,
        ContentType = signature.ContentType,
        ExpiresAt = signature.ExpiresAt
    };
}
