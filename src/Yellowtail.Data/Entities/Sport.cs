namespace Yellowtail.Data.Entities;

/// <summary>
/// Represents a sport in the global catalog, shared across all branches.
/// </summary>
public class Sport
{
    /// <summary>
    /// The unique identifier of the sport.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The display name of the sport.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The members associated with this sport, via the <see cref="MemberSport"/> join entity.
    /// </summary>
    public ICollection<MemberSport> MemberSports { get; set; } = new List<MemberSport>();
}
