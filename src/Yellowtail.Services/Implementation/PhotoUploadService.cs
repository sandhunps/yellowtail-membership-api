using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
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
    /// The logger used to record signature generation. Never logs the actual URL or signature,
    /// since those are live, short-lived credentials.
    /// </summary>
    private readonly ILogger<PhotoUploadService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PhotoUploadService"/> class.
    /// </summary>
    /// <param name="options">The R2 credentials and upload defaults.</param>
    /// <param name="logger">The logger used to record signature generation.</param>
    public PhotoUploadService(IOptions<R2Options> options, ILogger<PhotoUploadService> logger)
    {
        _options = options.Value;
        _logger = logger;
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
        #region LLD
        // Step 1: Resolve the content type for the given file extension, rejecting anything
        //         unsupported before doing any further work.
        // Step 2: Build a unique object key from the configured upload folder + a new GUID +
        //         the normalized extension.
        // Step 3: Compute the expiry timestamp the pre-signed URL will be valid until.
        // Step 4: Ask the S3-compatible client to compute the pre-signed PUT URL for that key,
        //         content type, and expiry.
        // Step 5: Log the signature generation at Information level (key, content type, expiry
        //         only - never the actual URL/signature, since those are live credentials).
        // Step 6: Build and return the PhotoUploadSignature: the pre-signed upload URL, the
        //         resulting public read URL, the content type, and the expiry.
        #endregion
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

        _logger.LogInformation(
            "Generated upload signature for key {Key} ({ContentType}), expires {ExpiresAt}",
            key, contentType, expiresAt);

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
    private static string ResolveContentType(string fileExtension)
    {
        #region LLD
        // Step 1: Normalize the extension: strip a leading dot if present, lowercase it.
        // Step 2: Map known extensions (jpg/jpeg, png, webp) to their MIME content type.
        // Step 3: For anything else, throw ValidationFailedException so the caller (and
        //         ultimately the API) rejects the request with a 400 before any upload URL
        //         is ever generated.
        #endregion
        return fileExtension.TrimStart('.').ToLowerInvariant() switch
        {
            "jpg" or "jpeg" => "image/jpeg",
            "png" => "image/png",
            "webp" => "image/webp",
            _ => throw new ValidationFailedException(
                $"Unsupported file extension '{fileExtension}'. Use jpg, jpeg, png, or webp.")
        };
    }
}
