using Microsoft.Extensions.Logging;
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
    /// The logger used to record member operations and the business-rule failures behind them.
    /// </summary>
    private readonly ILogger<MemberService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemberService"/> class.
    /// </summary>
    /// <param name="repository">The repository used to read and persist members.</param>
    /// <param name="logger">The logger used to record member operations and the business-rule failures behind them.</param>
    public MemberService(IMemberRepository repository, ILogger<MemberService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<PagedResult<Member>> GetAllAsync(MemberListQuery query)
    {
        var result = await _repository.GetAllAsync(new MemberQuery
        {
            SportId = query.SportId,
            IsActive = query.IsActive,
            NameSearch = query.NameSearch,
            JoinedFrom = query.JoinedFrom,
            JoinedTo = query.JoinedTo,
            Page = query.Page,
            PageSize = query.PageSize
        });

        _logger.LogDebug(
            "Listed members: {ReturnedCount} of {TotalCount} (page {Page}, sportId {SportId}, isActive {IsActive}, nameSearch {NameSearch})",
            result.Items.Count, result.TotalCount, query.Page, query.SportId, query.IsActive, query.NameSearch);

        return result;
    }

    /// <inheritdoc/>
    public async Task<Member> GetByIdAsync(Guid id)
    {
        var member = await _repository.GetByIdAsync(id);
        if (member is null)
        {
            throw new NotFoundException($"Member '{id}' was not found.");
        }

        return member;
    }

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

        _logger.LogInformation(
            "Created member {MemberId} ({Email}) with {SportCount} sport(s)",
            member.Id, member.Email, input.SportIds.Count);

        return await _repository.GetByIdAsync(member.Id) ?? member;
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(Guid id, MemberUpdateInput input)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
        {
            throw new NotFoundException($"Member '{id}' was not found.");
        }

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

        _logger.LogInformation(
            "Updated member {MemberId}: isActive {IsActive}, {SportCount} sport(s)",
            id, input.IsActive, input.SportIds.Count);
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Guid id)
    {
        var deleted = await _repository.SoftDeleteAsync(id);
        if (!deleted)
        {
            throw new NotFoundException($"Member '{id}' was not found.");
        }

        _logger.LogInformation("Soft-deleted member {MemberId}", id);
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
