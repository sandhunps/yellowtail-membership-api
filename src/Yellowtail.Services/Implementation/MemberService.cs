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
        #region LLD
        // Step 1: Map the incoming service-layer query (MemberListQuery) to the data-layer
        //         query shape (MemberQuery) the repository expects.
        // Step 2: Ask the repository for the matching page of members plus the total count.
        // Step 3: Log the result counts and filter criteria at Debug level for diagnostics.
        // Step 4: Return the paged result to the caller.
        #endregion
        var result = await _repository.GetAllAsync(new MemberQuery
        {
            SportId = query.SportId,
            IsActive = query.IsActive,
            NameSearch = query.NameSearch,
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
        #region LLD
        // Step 1: Fetch the member by id from the repository, regardless of active status.
        // Step 2: If no member is found, throw NotFoundException so the API layer maps it to 404.
        // Step 3: Return the found member.
        #endregion
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
        #region LLD
        // Step 1: Validate that the email and phone number aren't already used by another
        //         active member.
        // Step 2: Validate that every requested sport id exists in the catalog.
        // Step 3: Build a new Member entity from the input: generate a new id, default
        //         IsActive to true, and stamp JoinedOn as today.
        // Step 4: Persist the new member via the repository.
        // Step 5: If any sports were requested, associate them with the new member.
        // Step 6: Log the creation at Information level.
        // Step 7: Re-fetch the member (so the response includes its loaded sport
        //         associations) and return it, falling back to the in-memory instance
        //         if the re-fetch somehow returns nothing.
        #endregion
        await EnsureEmailAndPhoneAreUniqueAsync(input.Email, input.Phone, excludeMemberId: null);
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
        #region LLD
        // Step 1: Fetch the existing member by id; throw NotFoundException if it doesn't exist.
        // Step 2: Validate that the email and phone number aren't already used by a
        //         *different* active member (the member being updated is excluded from
        //         its own uniqueness check).
        // Step 3: Validate that every requested sport id exists in the catalog.
        // Step 4: Apply the input values onto the existing tracked member entity.
        // Step 5: Persist the updated member via the repository.
        // Step 6: Replace the member's sport associations with the requested set.
        // Step 7: Log the update at Information level.
        #endregion
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
        {
            throw new NotFoundException($"Member '{id}' was not found.");
        }

        await EnsureEmailAndPhoneAreUniqueAsync(input.Email, input.Phone, excludeMemberId: id);
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
        #region LLD
        // Step 1: Ask the repository to soft-delete the member (mark IsActive = false).
        // Step 2: If no matching member was found, throw NotFoundException.
        // Step 3: Log the soft-delete at Information level.
        #endregion
        var deleted = await _repository.SoftDeleteAsync(id);
        if (!deleted)
        {
            throw new NotFoundException($"Member '{id}' was not found.");
        }

        _logger.LogInformation("Soft-deleted member {MemberId}", id);
    }

    /// <summary>
    /// Validates that no other active member already has the given email or phone number.
    /// Applies on both create and update, since checking only on create would let an update
    /// silently collide with an email/phone already in use by a different member.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <param name="phone">The phone number to validate, if provided. Not checked when absent.</param>
    /// <param name="excludeMemberId">
    /// On update, the member being updated is excluded from the check, so keeping one's own
    /// email/phone unchanged doesn't collide with oneself. <see langword="null"/> on create.
    /// </param>
    /// <exception cref="ValidationFailedException">Another active member already has this email or phone number.</exception>
    private async Task EnsureEmailAndPhoneAreUniqueAsync(string email, string? phone, Guid? excludeMemberId)
    {
        #region LLD
        // Step 1: Check whether another active member already has this email (case-insensitive);
        //         throw ValidationFailedException if so.
        // Step 2: If a phone number was provided, check whether another active member already
        //         has it; throw ValidationFailedException if so. Skipped entirely when phone
        //         is null/empty, since phone is optional and shouldn't force uniqueness on "no phone".
        #endregion
        if (await _repository.EmailExistsAsync(email, excludeMemberId))
        {
            throw new ValidationFailedException($"Email '{email}' is already in use by another member.");
        }

        if (!string.IsNullOrWhiteSpace(phone) && await _repository.PhoneExistsAsync(phone, excludeMemberId))
        {
            throw new ValidationFailedException($"Phone number '{phone}' is already in use by another member.");
        }
    }

    /// <summary>
    /// Validates that every given sport identifier exists in the catalog.
    /// </summary>
    /// <param name="sportIds">The sport identifiers to validate.</param>
    /// <exception cref="ValidationFailedException">One of the given sport identifiers does not exist.</exception>
    private async Task EnsureSportsExistAsync(IReadOnlyList<Guid> sportIds)
    {
        #region LLD
        // Step 1: For each requested sport id, ask the repository whether it exists in the catalog.
        // Step 2: On the first sport id that doesn't exist, throw ValidationFailedException and
        //         stop checking the rest (fail fast).
        #endregion
        foreach (var sportId in sportIds)
        {
            if (!await _repository.SportExistsAsync(sportId))
            {
                throw new ValidationFailedException($"Sport '{sportId}' does not exist.");
            }
        }
    }
}
