using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using Yellowtail.Services.Configuration;
using Yellowtail.Services.Contracts;
using Yellowtail.Services.Exceptions;
using Yellowtail.Services.Models;

namespace Yellowtail.Services.Implementation;

/// <summary>
/// Default implementation of <see cref="IPhotoUploadService"/>, backed by the AWS S3 SDK
/// pointed at Cloudflare R2's S3-compatible API.
/// </summary>
public class PhotoUploadService : IPhotoUploadService
{
    /// <summary>
    /// How long a generated upload URL remains valid.
    /// </summary>
    private static readonly TimeSpan SignatureLifetime = TimeSpan.FromMinutes(5);

    /// <summary>
    /// The R2 credentials and upload defaults.
    /// </summary>
    private readonly R2Options _options;

    /// <summary>
    /// The S3-compatible client used to compute pre-signed URLs.
    /// </summary>
    private readonly IAmazonS3 _s3Client;

    /// <summary>
    /// Initializes a new instance of the <see cref="PhotoUploadService"/> class.
    /// </summary>
    /// <param name="options">The R2 credentials and upload defaults.</param>
    public PhotoUploadService(IOptions<R2Options> options)
    {
        _options = options.Value;
        _s3Client = new AmazonS3Client(_options.AccessKeyId, _options.SecretAccessKey, new AmazonS3Config
        {
            ServiceURL = $"https://{_options.AccountId}.r2.cloudflarestorage.com",
            AuthenticationRegion = "auto",
            ForcePathStyle = true
        });
    }

    /// <inheritdoc/>
    public PhotoUploadSignature GenerateUploadSignature(string fileExtension)
    {
        var contentType = ResolveContentType(fileExtension);
        var key = $"{_options.UploadFolder}/{Guid.NewGuid()}.{fileExtension.TrimStart('.').ToLowerInvariant()}";
        var expiresAt = DateTimeOffset.UtcNow.Add(SignatureLifetime);

        var uploadUrl = _s3Client.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            Verb = HttpVerb.PUT,
            Expires = expiresAt.UtcDateTime,
            ContentType = contentType
        });

        return new PhotoUploadSignature
        {
            UploadUrl = uploadUrl,
            PublicUrl = $"{_options.PublicBaseUrl.TrimEnd('/')}/{key}",
            ContentType = contentType,
            ExpiresAt = expiresAt
        };
    }

    /// <summary>
    /// Maps a supported image file extension to its MIME content type.
    /// </summary>
    /// <param name="fileExtension">The file extension (without a leading dot) to resolve.</param>
    /// <returns>The corresponding content type.</returns>
    /// <exception cref="ValidationFailedException"><paramref name="fileExtension"/> is not a supported image type.</exception>
    private static string ResolveContentType(string fileExtension) =>
        fileExtension.TrimStart('.').ToLowerInvariant() switch
        {
            "jpg" or "jpeg" => "image/jpeg",
            "png" => "image/png",
            "webp" => "image/webp",
            _ => throw new ValidationFailedException(
                $"Unsupported file extension '{fileExtension}'. Use jpg, jpeg, png, or webp.")
        };
}
