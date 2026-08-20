using Yellowtail.Data.Enums;

namespace Yellowtail.API.Contracts.Members;

/// <summary>
/// Represents a request to create a new member.
/// </summary>
public class CreateMemberRequest
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
    /// The member's role. Defaults to the standard member role if not specified.
    /// </summary>
    public MemberRole? Role { get; set; }

    /// <summary>
    /// The identifiers of the sports to associate with the member.
    /// </summary>
    public List<Guid>? SportIds { get; set; }
}
