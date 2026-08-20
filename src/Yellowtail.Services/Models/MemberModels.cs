using Yellowtail.Data.Enums;

namespace Yellowtail.Services.Models;

/// <summary>
/// Carries the data needed to create a new member.
/// </summary>
public class MemberCreateInput
{
    /// <summary>
    /// The member's first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// The member's last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// The member's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The member's phone number, if provided.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// The member's date of birth, if provided.
    /// </summary>
    public DateOnly? DateOfBirth { get; set; }

    /// <summary>
    /// The URL of the member's photo, if provided.
    /// </summary>
    public string? PhotoUrl { get; set; }

    /// <summary>
    /// The member's role within the club.
    /// </summary>
    public MemberRole Role { get; set; }

    /// <summary>
    /// The identifiers of the sports to associate with the member.
    /// </summary>
    public IReadOnlyList<Guid> SportIds { get; set; } = new List<Guid>();
}

/// <summary>
/// Carries the data needed to update an existing member.
/// </summary>
public class MemberUpdateInput
{
    /// <summary>
    /// The member's first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// The member's last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// The member's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The member's phone number, if provided.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// The member's date of birth, if provided.
    /// </summary>
    public DateOnly? DateOfBirth { get; set; }

    /// <summary>
    /// The URL of the member's photo, if provided.
    /// </summary>
    public string? PhotoUrl { get; set; }

    /// <summary>
    /// The member's role within the club.
    /// </summary>
    public MemberRole Role { get; set; }

    /// <summary>
    /// Whether the member should be active. Setting this to <see langword="false"/> is
    /// equivalent to a soft delete; setting a previously inactive member back to
    /// <see langword="true"/> reactivates it.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// The identifiers of the sports the member should be associated with. Replaces any existing associations.
    /// </summary>
    public IReadOnlyList<Guid> SportIds { get; set; } = new List<Guid>();
}

/// <summary>
/// Carries the filter and pagination criteria for listing members, as requested through the service layer.
/// </summary>
public class MemberListQuery
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
    /// If set, restricts results to members whose first or last name contains this value.
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
