using Yellowtail.Data.Enums;

namespace Yellowtail.API.Contracts.Members;

/// <summary>
/// Represents a request to update an existing member.
/// </summary>
public class UpdateMemberRequest
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
    /// The member's role.
    /// </summary>
    public MemberRole Role { get; set; }

    /// <summary>
    /// Whether the member is currently active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// The identifiers of the sports to associate with the member.
    /// </summary>
    public List<Guid>? SportIds { get; set; }
}
