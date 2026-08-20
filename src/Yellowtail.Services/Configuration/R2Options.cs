namespace Yellowtail.Services.Configuration;

/// <summary>
/// Credentials and defaults for generating pre-signed Cloudflare R2 upload URLs.
/// Bound from the "R2" configuration section. <see cref="AccessKeyId"/> and
/// <see cref="SecretAccessKey"/> are genuine third-party credentials and must only ever come
/// from User Secrets (local dev) or environment variables/a real secret manager (elsewhere) —
/// never from a committed appsettings file.
/// </summary>
public class R2Options
{
    /// <summary>
    /// The configuration section name this options class binds to.
    /// </summary>
    public const string SectionName = "R2";

    /// <summary>
    /// The Cloudflare account ID, used to build the R2 S3-compatible endpoint
    /// (<c>https://{AccountId}.r2.cloudflarestorage.com</c>). Not sensitive.
    /// </summary>
    public string AccountId { get; set; } = string.Empty;

    /// <summary>
    /// The name of the R2 bucket to upload into. Not sensitive.
    /// </summary>
    public string BucketName { get; set; } = string.Empty;

    /// <summary>
    /// The public base URL for reading uploaded objects back (the bucket's R2.dev public
    /// development URL, or a connected custom domain), with no trailing slash. Not sensitive.
    /// </summary>
    public string PublicBaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// The R2 API token's Access Key ID.
    /// </summary>
    public string AccessKeyId { get; set; } = string.Empty;

    /// <summary>
    /// The R2 API token's Secret Access Key. Never sent to the client and never committed to source control.
    /// </summary>
    public string SecretAccessKey { get; set; } = string.Empty;

    /// <summary>
    /// The folder (object key prefix) member photos are uploaded into.
    /// </summary>
    public string UploadFolder { get; set; } = "members";
}
