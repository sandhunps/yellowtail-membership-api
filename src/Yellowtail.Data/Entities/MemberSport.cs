namespace Yellowtail.Data.Entities;

/// <summary>
/// Represents the many-to-many association between a <see cref="Entities.Member"/> and a <see cref="Entities.Sport"/>.
/// </summary>
public class MemberSport
{
    /// <summary>
    /// The identifier of the associated member.
    /// </summary>
    public Guid MemberId { get; set; }

    /// <summary>
    /// The associated member.
    /// </summary>
    public Member Member { get; set; } = null!;

    /// <summary>
    /// The identifier of the associated sport.
    /// </summary>
    public Guid SportId { get; set; }

    /// <summary>
    /// The associated sport.
    /// </summary>
    public Sport Sport { get; set; } = null!;
}
