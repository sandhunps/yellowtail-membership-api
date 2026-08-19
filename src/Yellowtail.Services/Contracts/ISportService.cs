using Yellowtail.Data.Entities;

namespace Yellowtail.Services.Contracts;

/// <summary>
/// Provides business logic for reading the sports catalog.
/// </summary>
public interface ISportService
{
    /// <summary>
    /// Gets all sports in the catalog.
    /// </summary>
    /// <returns>The full list of sports.</returns>
    Task<IReadOnlyList<Sport>> GetAllAsync();
}
