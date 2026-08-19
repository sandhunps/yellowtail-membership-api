using Microsoft.AspNetCore.Mvc;
using Yellowtail.API.Contracts;
using Yellowtail.Services;

namespace Yellowtail.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembersController(IMemberService memberService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MemberResponse>>> GetAll()
    {
        var members = await memberService.GetAllAsync();
        return Ok(members.Select(MemberResponse.FromEntity));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MemberResponse>> GetById(Guid id)
    {
        var member = await memberService.GetByIdAsync(id);
        return member is null ? NotFound() : Ok(MemberResponse.FromEntity(member));
    }

    [HttpPost]
    public async Task<ActionResult<MemberResponse>> Create(CreateMemberRequest request)
    {
        var member = await memberService.CreateAsync(request.FirstName, request.LastName, request.Email);
        var response = MemberResponse.FromEntity(member);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateMemberRequest request)
    {
        var updated = await memberService.UpdateAsync(id, request.FirstName, request.LastName, request.Email, request.IsActive);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await memberService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
