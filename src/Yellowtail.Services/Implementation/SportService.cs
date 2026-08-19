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
    /// Initializes a new instance of the <see cref="SportService"/> class.
    /// </summary>
    /// <param name="repository">The repository used to read sports.</param>
    public SportService(ISportRepository repository)
    {
        _repository = repository;
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<Sport>> GetAllAsync() => _repository.GetAllAsync();
}
