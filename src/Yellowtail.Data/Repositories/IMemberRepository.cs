using Yellowtail.Data.Entities;

namespace Yellowtail.Data.Repositories;

/// <summary>
/// Represents the filter and pagination criteria for listing members.
/// </summary>
public class MemberQuery
{
    /// <summary>
    /// If set, restricts results to members associated with this sport.
    /// </summary>
    public Guid? SportId { get; set; }

    /// <summary>
    /// If <see langword="null"/> or <see langword="true"/>, only active members are returned.
    /// If <see langword="false"/>, only inactive (soft-deleted) members are returned.
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// If set, restricts results to members whose first or last name contains this value (case-insensitive).
    /// </summary>
    public string? NameSearch { get; set; }

    /// <summary>
    /// The page number to retrieve, starting at 1.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// The number of items per page.
    /// </summary>
    public int PageSize { get; set; }
}

/// <summary>
/// Represents a single page of results, along with the total count across all pages.
/// </summary>
/// <typeparam name="T">The type of item contained in the page.</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// The items on the current page.
    /// </summary>
    public IReadOnlyList<T> Items { get; set; } = new List<T>();

    /// <summary>
    /// The total number of items across all pages.
    /// </summary>
    public int TotalCount { get; set; }
}

/// <summary>
/// Provides data access for <see cref="Member"/> entities.
/// </summary>
public interface IMemberRepository
{
    /// <summary>
    /// Gets a paginated, filtered list of members.
    /// </summary>
    /// <param name="query">The filter and pagination criteria.</param>
    /// <returns>The matching page of members and the total count across all pages.</returns>
    Task<PagedResult<Member>> GetAllAsync(MemberQuery query);

    /// <summary>
    /// Gets a single member by identifier, regardless of active status.
    /// </summary>
    /// <param name="id">The unique identifier of the member.</param>
    /// <returns>The member, or <see langword="null"/> if no member with the given identifier exists.</returns>
    Task<Member?> GetByIdAsync(Guid id);

    /// <summary>
    /// Adds a new member.
    /// </summary>
    /// <param name="member">The member to add.</param>
    Task AddAsync(Member member);

    /// <summary>
    /// Updates an existing member.
    /// </summary>
    /// <param name="member">The member with updated values. Its <see cref="Member.Id"/> must match an existing member.</param>
    /// <returns><see langword="true"/> if a matching member was found and updated; otherwise <see langword="false"/>.</returns>
    Task<bool> UpdateAsync(Member member);

    /// <summary>
    /// Marks a member as inactive rather than removing the row (soft delete).
    /// </summary>
    /// <param name="id">The unique identifier of the member to soft-delete.</param>
    /// <returns><see langword="true"/> if a matching member was found and soft-deleted; otherwise <see langword="false"/>.</returns>
    Task<bool> SoftDeleteAsync(Guid id);

    /// <summary>
    /// Checks whether a sport with the given identifier exists in the catalog.
    /// </summary>
    /// <param name="sportId">The unique identifier of the sport.</param>
    /// <returns><see langword="true"/> if the sport exists; otherwise <see langword="false"/>.</returns>
    Task<bool> SportExistsAsync(Guid sportId);

    /// <summary>
    /// Checks whether an active member already has the given email address (case-insensitive).
    /// </summary>
    /// <param name="email">The email address to check.</param>
    /// <param name="excludeMemberId">If set, a member with this identifier is not considered a match (for update scenarios).</param>
    /// <returns><see langword="true"/> if another active member already has this email; otherwise <see langword="false"/>.</returns>
    Task<bool> EmailExistsAsync(string email, Guid? excludeMemberId = null);

    /// <summary>
    /// Checks whether an active member already has the given phone number.
    /// </summary>
    /// <param name="phone">The phone number to check.</param>
    /// <param name="excludeMemberId">If set, a member with this identifier is not considered a match (for update scenarios).</param>
    /// <returns><see langword="true"/> if another active member already has this phone number; otherwise <see langword="false"/>.</returns>
    Task<bool> PhoneExistsAsync(string phone, Guid? excludeMemberId = null);

    /// <summary>
    /// Replaces a member's sport associations with the given set of sport identifiers.
    /// </summary>
    /// <param name="memberId">The unique identifier of the member.</param>
    /// <param name="sportIds">
    /// The identifiers of the sports the member should be associated with. Any existing
    /// associations not in this set are removed.
    /// </param>
    Task ReplaceMemberSportsAsync(Guid memberId, IEnumerable<Guid> sportIds);
}
