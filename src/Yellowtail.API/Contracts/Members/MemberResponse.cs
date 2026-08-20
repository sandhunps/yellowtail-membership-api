using Yellowtail.API.Contracts;
using Yellowtail.Data.Entities;
using Yellowtail.Data.Enums;

namespace Yellowtail.API.Contracts.Members;

/// <summary>
/// Represents a member in API responses.
/// </summary>
public class MemberResponse
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
    /// The date the member joined.
    /// </summary>
    public DateOnly JoinedOn { get; set; }

    /// <summary>
    /// The sports the member is associated with.
    /// </summary>
    public IReadOnlyList<SportResponse> Sports { get; set; } = new List<SportResponse>();

    /// <summary>
    /// Creates a <see cref="MemberResponse"/> from a <see cref="Member"/> entity.
    /// </summary>
    /// <param name="member">The source entity.</param>
    /// <returns>The mapped response.</returns>
    public static MemberResponse FromEntity(Member member) => new()
    {
        Id = member.Id,
        FirstName = member.FirstName,
        LastName = member.LastName,
        Email = member.Email,
        Phone = member.Phone,
        DateOfBirth = member.DateOfBirth,
        PhotoUrl = member.PhotoUrl,
        Role = member.Role,
        IsActive = member.IsActive,
        JoinedOn = member.JoinedOn,
        Sports = member.MemberSports.Select(ms => SportResponse.FromEntity(ms.Sport)).ToList()
    };
}
