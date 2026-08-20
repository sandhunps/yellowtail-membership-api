using Microsoft.EntityFrameworkCore;
using Yellowtail.Data.Entities;

namespace Yellowtail.Data.Repositories;

/// <summary>
/// Entity Framework Core-backed implementation of <see cref="IMemberRepository"/>.
/// </summary>
public class MemberRepository : IMemberRepository
{
    /// <summary>
    /// The database context used to query and persist members.
    /// </summary>
    private readonly YellowtailDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemberRepository"/> class.
    /// </summary>
    /// <param name="context">The database context to query and persist changes through.</param>
    public MemberRepository(YellowtailDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<PagedResult<Member>> GetAllAsync(MemberQuery query)
    {
        // Default (null/true) relies on Member's HasQueryFilter to show active-only.
        // isActive=false is the explicit override to see soft-deleted/inactive members.
        var members = query.IsActive == false
            ? _context.Members.IgnoreQueryFilters().Where(m => !m.IsActive)
            : _context.Members;

        if (query.SportId is { } sportId)
        {
            members = members.Where(m => m.MemberSports.Any(ms => ms.SportId == sportId));
        }

        if (!string.IsNullOrWhiteSpace(query.NameSearch))
        {
            var search = query.NameSearch.Trim();
            members = members.Where(m =>
                EF.Functions.ILike(m.FirstName, $"%{search}%") ||
                EF.Functions.ILike(m.LastName, $"%{search}%"));
        }

        var totalCount = await members.CountAsync();

        var items = await members
            .Include(m => m.MemberSports)
            .ThenInclude(ms => ms.Sport)
            .OrderBy(m => m.FirstName)
            .ThenBy(m => m.LastName)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new PagedResult<Member>
        {
            Items = items,
            TotalCount = totalCount
        };
    }

    /// <inheritdoc/>
    public Task<Member?> GetByIdAsync(Guid id)
    {
        return _context.Members
            .IgnoreQueryFilters()
            .Include(m => m.MemberSports)
            .ThenInclude(ms => ms.Sport)
            .SingleOrDefaultAsync(m => m.Id == id);
    }

    /// <inheritdoc/>
    public async Task AddAsync(Member member)
    {
        _context.Members.Add(member);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateAsync(Member member)
    {
        var exists = await _context.Members.IgnoreQueryFilters().AnyAsync(m => m.Id == member.Id);
        if (!exists)
        {
            return false;
        }

        _context.Members.Update(member);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> SoftDeleteAsync(Guid id)
    {
        var member = await _context.Members.IgnoreQueryFilters().SingleOrDefaultAsync(m => m.Id == id);
        if (member is null)
        {
            return false;
        }

        member.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc/>
    public Task<bool> SportExistsAsync(Guid sportId) =>
        _context.Sports.AnyAsync(s => s.Id == sportId);

    /// <inheritdoc/>
    public async Task ReplaceMemberSportsAsync(Guid memberId, IEnumerable<Guid> sportIds)
    {
        var existing = _context.MemberSports.Where(ms => ms.MemberId == memberId);
        _context.MemberSports.RemoveRange(existing);

        foreach (var sportId in sportIds.Distinct())
        {
            _context.MemberSports.Add(new MemberSport { MemberId = memberId, SportId = sportId });
        }

        await _context.SaveChangesAsync();
    }
}
