using Microsoft.EntityFrameworkCore;
using Yellowtail.Data.Entities;

namespace Yellowtail.Data.Repositories;

/// <summary>
/// Entity Framework Core-backed implementation of <see cref="ISportRepository"/>.
/// </summary>
public class SportRepository : ISportRepository
{
    /// <summary>
    /// The database context used to query sports.
    /// </summary>
    private readonly YellowtailDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="SportRepository"/> class.
    /// </summary>
    /// <param name="context">The database context to query through.</param>
    public SportRepository(YellowtailDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Sport>> GetAllAsync() =>
        await _context.Sports.OrderBy(s => s.Name).ToListAsync();

    /// <inheritdoc/>
    public Task<Sport?> GetByIdAsync(Guid id) =>
        _context.Sports.SingleOrDefaultAsync(s => s.Id == id);
}
