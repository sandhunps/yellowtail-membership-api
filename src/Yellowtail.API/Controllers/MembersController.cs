using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Yellowtail.API.Configuration;
using Yellowtail.API.Contracts.Members;
using Yellowtail.Data.Enums;
using Yellowtail.Services.Contracts;
using Yellowtail.Services.Models;

namespace Yellowtail.API.Controllers;

/// <summary>
/// Exposes CRUD and listing endpoints for members. Request validation is handled globally by
/// <see cref="Filters.ValidationFilter"/>, so actions can assume incoming requests are already valid.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MembersController : ControllerBase
{
    /// <summary>
    /// The service used to read and persist members.
    /// </summary>
    private readonly IMemberService _memberService;

    /// <summary>
    /// The configured default and maximum page sizes for the members list.
    /// </summary>
    private readonly PaginationOptions _paginationOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="MembersController"/> class.
    /// </summary>
    /// <param name="memberService">The service used to read and persist members.</param>
    /// <param name="paginationOptions">The configured default and maximum page sizes for the members list.</param>
    public MembersController(
        IMemberService memberService,
        IOptions<PaginationOptions> paginationOptions)
    {
        _memberService = memberService;
        _paginationOptions = paginationOptions.Value;
    }

    /// <summary>
    /// Gets a paginated, optionally filtered list of members.
    /// </summary>
    /// <param name="filter">The filter and pagination criteria, bound from the query string.</param>
    /// <returns>The matching members for the requested page.</returns>
    [HttpGet]
    public async Task<ActionResult<MemberListResponse>> GetAll([FromQuery] MemberListFilterRequest filter)
    {
        var effectivePage = Math.Max(filter.Page ?? 1, 1);
        var effectivePageSize = Math.Clamp(
            filter.PageSize ?? _paginationOptions.DefaultPageSize,
            1,
            _paginationOptions.MaxPageSize);

        var result = await _memberService.GetAllAsync(new MemberListQuery
        {
            SportId = filter.SportId,
            IsActive = filter.IsActive,
            NameSearch = filter.Name,
            Page = effectivePage,
            PageSize = effectivePageSize
        });

        return Ok(new MemberListResponse
        {
            Items = result.Items.Select(MemberResponse.FromEntity).ToList(),
            TotalCount = result.TotalCount,
            Page = effectivePage,
            PageSize = effectivePageSize
        });
    }

    /// <summary>
    /// Gets a single member by identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the member.</param>
    /// <returns>The requested member.</returns>
    /// <exception cref="Yellowtail.Services.Exceptions.NotFoundException">No member with the given identifier exists.</exception>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MemberResponse>> GetById(Guid id)
    {
        var member = await _memberService.GetByIdAsync(id);
        return Ok(MemberResponse.FromEntity(member));
    }

    /// <summary>
    /// Creates a new member.
    /// </summary>
    /// <param name="request">The details of the member to create.</param>
    /// <returns>The identifier of the created member.</returns>
    /// <exception cref="Yellowtail.Services.Exceptions.ValidationFailedException">One or more of the requested sport identifiers does not exist.</exception>
    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateMemberRequest request)
    {
        var member = await _memberService.CreateAsync(new MemberCreateInput
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            DateOfBirth = request.DateOfBirth,
            PhotoUrl = request.PhotoUrl,
            Role = request.Role ?? MemberRole.Member,
            SportIds = request.SportIds ?? []
        });

        return Ok(member.Id);
    }

    /// <summary>
    /// Updates an existing member.
    /// </summary>
    /// <param name="id">The unique identifier of the member to update.</param>
    /// <param name="request">The updated details of the member.</param>
    /// <returns>No content on success.</returns>
    /// <exception cref="Yellowtail.Services.Exceptions.NotFoundException">No member with the given identifier exists.</exception>
    /// <exception cref="Yellowtail.Services.Exceptions.ValidationFailedException">One or more of the requested sport identifiers does not exist.</exception>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateMemberRequest request)
    {
        await _memberService.UpdateAsync(id, new MemberUpdateInput
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            DateOfBirth = request.DateOfBirth,
            PhotoUrl = request.PhotoUrl,
            Role = request.Role,
            IsActive = request.IsActive,
            SportIds = request.SportIds ?? []
        });

        return NoContent();
    }

    /// <summary>
    /// Deletes a member. This is a soft delete: the member is marked inactive rather than removed.
    /// </summary>
    /// <param name="id">The unique identifier of the member to delete.</param>
    /// <returns>No content on success.</returns>
    /// <exception cref="Yellowtail.Services.Exceptions.NotFoundException">No member with the given identifier exists.</exception>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _memberService.DeleteAsync(id);
        return NoContent();
    }
}
