namespace Yellowtail.API.Contracts.Members;

/// <summary>
/// Represents a paginated list of members.
/// </summary>
public class MemberListResponse
{
    /// <summary>
    /// The members on the current page.
    /// </summary>
    public IReadOnlyList<MemberResponse> Items { get; set; } = new List<MemberResponse>();

    /// <summary>
    /// The total number of members across all pages.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// The current page number.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// The number of items per page.
    /// </summary>
    public int PageSize { get; set; }
}
