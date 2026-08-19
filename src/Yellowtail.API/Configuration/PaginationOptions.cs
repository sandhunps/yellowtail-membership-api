namespace Yellowtail.API.Configuration;

/// <summary>
/// Configures default and maximum page sizes for paginated list endpoints.
/// Bound from the "Pagination" section of appsettings.json.
/// </summary>
public class PaginationOptions
{
    /// <summary>
    /// The configuration section name this options class binds to.
    /// </summary>
    public const string SectionName = "Pagination";

    /// <summary>
    /// The page size used when a request does not specify one.
    /// </summary>
    public int DefaultPageSize { get; set; } = 20;

    /// <summary>
    /// The largest page size a request is allowed to specify. Requested sizes above this are clamped down.
    /// </summary>
    public int MaxPageSize { get; set; } = 100;
}
