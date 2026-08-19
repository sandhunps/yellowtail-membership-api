using Yellowtail.Data.Entities;
using Yellowtail.Data.Repositories;
using Yellowtail.Services.Contracts;
using Yellowtail.Services.Exceptions;
using Yellowtail.Services.Models;

namespace Yellowtail.Services.Implementation;

/// <summary>
/// Default implementation of <see cref="IMemberService"/>.
/// </summary>
public class MemberService : IMemberService
{
    /// <summary>
    /// The repository used to read and persist members.
    /// </summary>
    private readonly IMemberRepository _repository;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemberService"/> class.
    /// </summary>
    /// <param name="repository">The repository used to read and persist members.</param>
    public MemberService(IMemberRepository repository)
    {
        _repository = repository;
    }

    /// <inheritdoc/>
    public Task<PagedResult<Member>> GetAllAsync(MemberListQuery query) =>
        _repository.GetAllAsync(new MemberQuery
        {
            SportId = query.SportId,
            IsActive = query.IsActive,
            NameSearch = query.NameSearch,
            JoinedFrom = query.JoinedFrom,
            JoinedTo = query.JoinedTo,
            Page = query.Page,
            PageSize = query.PageSize
        });

    /// <inheritdoc/>
    public async Task<Member> GetByIdAsync(Guid id) =>
        await _repository.GetByIdAsync(id) ?? throw new NotFoundException($"Member '{id}' was not found.");

    /// <inheritdoc/>
    public async Task<Member> CreateAsync(MemberCreateInput input)
    {
        await EnsureSportsExistAsync(input.SportIds);

        var member = new Member
        {
            Id = Guid.NewGuid(),
            FirstName = input.FirstName,
            LastName = input.LastName,
            Email = input.Email,
            Phone = input.Phone,
            DateOfBirth = input.DateOfBirth,
            PhotoUrl = input.PhotoUrl,
            Role = input.Role,
            IsActive = true,
            JoinedOn = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        await _repository.AddAsync(member);

        if (input.SportIds.Count > 0)
        {
            await _repository.ReplaceMemberSportsAsync(member.Id, input.SportIds);
        }

        return await _repository.GetByIdAsync(member.Id) ?? member;
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(Guid id, MemberUpdateInput input)
    {
        var existing = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Member '{id}' was not found.");

        await EnsureSportsExistAsync(input.SportIds);

        existing.FirstName = input.FirstName;
        existing.LastName = input.LastName;
        existing.Email = input.Email;
        existing.Phone = input.Phone;
        existing.DateOfBirth = input.DateOfBirth;
        existing.PhotoUrl = input.PhotoUrl;
        existing.Role = input.Role;
        existing.IsActive = input.IsActive;

        await _repository.UpdateAsync(existing);
        await _repository.ReplaceMemberSportsAsync(id, input.SportIds);
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Guid id)
    {
        var deleted = await _repository.SoftDeleteAsync(id);
        if (!deleted)
        {
            throw new NotFoundException($"Member '{id}' was not found.");
        }
    }

    /// <summary>
    /// Validates that every given sport identifier exists in the catalog.
    /// </summary>
    /// <param name="sportIds">The sport identifiers to validate.</param>
    /// <exception cref="ValidationFailedException">One of the given sport identifiers does not exist.</exception>
    private async Task EnsureSportsExistAsync(IReadOnlyList<Guid> sportIds)
    {
        foreach (var sportId in sportIds)
        {
            if (!await _repository.SportExistsAsync(sportId))
            {
                throw new ValidationFailedException($"Sport '{sportId}' does not exist.");
            }
        }
    }
}
