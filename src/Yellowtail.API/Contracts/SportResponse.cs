using Yellowtail.Data.Entities;

namespace Yellowtail.API.Contracts;

/// <summary>
/// Represents a sport in API responses.
/// </summary>
public class SportResponse
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
    /// Creates a <see cref="SportResponse"/> from a <see cref="Sport"/> entity.
    /// </summary>
    /// <param name="sport">The source entity.</param>
    /// <returns>The mapped response.</returns>
    public static SportResponse FromEntity(Sport sport) => new()
    {
        Id = sport.Id,
        Name = sport.Name
    };
}
