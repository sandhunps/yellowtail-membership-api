namespace Yellowtail.Services.Models;

/// <summary>
/// Carries everything a client needs to upload one image directly to Cloudflare R2 using a
/// pre-signed URL, without the backend ever handling the image bytes.
/// </summary>
public class PhotoUploadSignature
{
    /// <summary>
    /// The pre-signed URL the client must send a single HTTP PUT to, with the raw image bytes
    /// as the request body and <see cref="ContentType"/> set as the Content-Type header.
    /// </summary>
    public string UploadUrl { get; set; } = string.Empty;

    /// <summary>
    /// The URL the uploaded image will be readable at afterward. Save this as the member's photo URL.
    /// </summary>
    public string PublicUrl { get; set; } = string.Empty;

    /// <summary>
    /// The exact Content-Type header the client's PUT request must send — it's bound into the
    /// signature, so a mismatched Content-Type causes R2 to reject the upload.
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// When the pre-signed URL stops being valid.
    /// </summary>
    public DateTimeOffset ExpiresAt { get; set; }
}
