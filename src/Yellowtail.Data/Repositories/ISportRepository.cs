using Yellowtail.Data.Entities;

namespace Yellowtail.Data.Repositories;

/// <summary>
/// Provides data access for <see cref="Sport"/> entities.
/// </summary>
public interface ISportRepository
{
    /// <summary>
    /// Gets all sports in the catalog, ordered alphabetically by name.
    /// </summary>
    /// <returns>The full list of sports.</returns>
    Task<IReadOnlyList<Sport>> GetAllAsync();

    /// <summary>
    /// Gets a single sport by identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the sport.</param>
    /// <returns>The sport, or <see langword="null"/> if no sport with the given identifier exists.</returns>
    Task<Sport?> GetByIdAsync(Guid id);
}
