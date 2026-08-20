namespace Yellowtail.API.Contracts.Members;

/// <summary>
/// Represents the filter and pagination criteria for a members list request, bound from the
/// query string. Consolidating these into one type keeps the controller action's signature
/// stable as more filters are added over time.
/// </summary>
public class MemberListFilterRequest
{
    /// <summary>
    /// If provided, restricts results to members associated with this sport.
    /// </summary>
    public Guid? SportId { get; set; }

    /// <summary>
    /// If provided, restricts results to members with this active status.
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// If provided, restricts results to members whose name matches this value.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The page number to retrieve. Defaults to 1 and is clamped to a minimum of 1.
    /// </summary>
    public int? Page { get; set; }

    /// <summary>
    /// The number of items per page. Defaults to and is clamped by the configured
    /// <see cref="Configuration.PaginationOptions"/>.
    /// </summary>
    public int? PageSize { get; set; }
}
