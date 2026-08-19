using Yellowtail.Data.Entities;
using Yellowtail.Data.Repositories;
using Yellowtail.Services.Models;

namespace Yellowtail.Services.Contracts;

/// <summary>
/// Provides business logic for managing members.
/// </summary>
public interface IMemberService
{
    /// <summary>
    /// Gets a paginated, filtered list of members.
    /// </summary>
    /// <param name="query">The filter and pagination criteria.</param>
    /// <returns>The matching page of members and the total count across all pages.</returns>
    Task<PagedResult<Member>> GetAllAsync(MemberListQuery query);

    /// <summary>
    /// Gets a single member by identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the member.</param>
    /// <returns>The requested member.</returns>
    /// <exception cref="Exceptions.NotFoundException">No member with the given identifier exists.</exception>
    Task<Member> GetByIdAsync(Guid id);

    /// <summary>
    /// Creates a new member.
    /// </summary>
    /// <param name="input">The details of the member to create.</param>
    /// <returns>The newly created member.</returns>
    /// <exception cref="Exceptions.ValidationFailedException">One or more of the requested sport identifiers does not exist.</exception>
    Task<Member> CreateAsync(MemberCreateInput input);

    /// <summary>
    /// Updates an existing member.
    /// </summary>
    /// <param name="id">The unique identifier of the member to update.</param>
    /// <param name="input">The updated details of the member.</param>
    /// <exception cref="Exceptions.NotFoundException">No member with the given identifier exists.</exception>
    /// <exception cref="Exceptions.ValidationFailedException">One or more of the requested sport identifiers does not exist.</exception>
    Task UpdateAsync(Guid id, MemberUpdateInput input);

    /// <summary>
    /// Soft-deletes a member by marking it inactive.
    /// </summary>
    /// <param name="id">The unique identifier of the member to delete.</param>
    /// <exception cref="Exceptions.NotFoundException">No member with the given identifier exists.</exception>
    Task DeleteAsync(Guid id);
}
