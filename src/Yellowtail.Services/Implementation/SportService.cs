using Microsoft.Extensions.Logging;
using Yellowtail.Data.Entities;
using Yellowtail.Data.Repositories;
using Yellowtail.Services.Contracts;

namespace Yellowtail.Services.Implementation;

/// <summary>
/// Default implementation of <see cref="ISportService"/>.
/// </summary>
public class SportService : ISportService
{
    /// <summary>
    /// The repository used to read sports.
    /// </summary>
    private readonly ISportRepository _repository;

    /// <summary>
    /// The logger used to record sport catalog reads.
    /// </summary>
    private readonly ILogger<SportService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SportService"/> class.
    /// </summary>
    /// <param name="repository">The repository used to read sports.</param>
    /// <param name="logger">The logger used to record sport catalog reads.</param>
    public SportService(ISportRepository repository, ILogger<SportService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Sport>> GetAllAsync()
    {
        #region LLD
        // Step 1: Fetch all sports from the repository (global catalog, no filtering).
        // Step 2: Log the count returned at Debug level.
        // Step 3: Return the list of sports.
        #endregion
        var sports = await _repository.GetAllAsync();
        _logger.LogDebug("Listed {Count} sports from the catalog", sports.Count);
        return sports;
    }
}
