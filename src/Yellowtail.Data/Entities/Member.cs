using Yellowtail.Data.Auditing;
using Yellowtail.Data.Enums;

namespace Yellowtail.Data.Entities;

/// <summary>
/// Represents a sports club member.
/// </summary>
public class Member : IAuditable
{
    /// <summary>
    /// The unique identifier of the member.
    /// </summary>
    public Guid Id { get; set; }

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
    /// The URL of the member's photo, if provided. The photo file itself is hosted
    /// externally (e.g. Cloudinary); only the URL is stored here.
    /// </summary>
    public string? PhotoUrl { get; set; }

    /// <summary>
    /// The member's role within the club.
    /// </summary>
    public MemberRole Role { get; set; } = MemberRole.Member;

    /// <summary>
    /// Whether the member is active. Set to <see langword="false"/> on soft delete;
    /// this same flag also drives the API's active/inactive list filter.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// The date the member joined.
    /// </summary>
    public DateOnly JoinedOn { get; set; }

    /// <summary>
    /// The sports this member is associated with, via the <see cref="MemberSport"/> join entity.
    /// </summary>
    public ICollection<MemberSport> MemberSports { get; set; } = new List<MemberSport>();

    /// <summary>
    /// The UTC date and time the member was created. Stamped automatically on insert.
    /// </summary>
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// The UTC date and time the member was last modified, or <see langword="null"/> if never updated since creation.
    /// </summary>
    public DateTime? ModifiedOn { get; set; }
}
