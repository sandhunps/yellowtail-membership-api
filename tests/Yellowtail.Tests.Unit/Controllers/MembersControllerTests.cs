using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using Yellowtail.API.Configuration;
using Yellowtail.API.Contracts;
using Yellowtail.API.Controllers;
using Yellowtail.Data.Entities;
using Yellowtail.Data.Enums;
using Yellowtail.Data.Repositories;
using Yellowtail.Services.Contracts;
using Yellowtail.Services.Models;

namespace Yellowtail.Tests.Unit.Controllers;

public class MembersControllerTests
{
    private readonly Mock<IMemberService> _memberService = new();
    private readonly MembersController _sut;

    public MembersControllerTests()
    {
        var options = Options.Create(new PaginationOptions { DefaultPageSize = 20, MaxPageSize = 100 });
        _sut = new MembersController(_memberService.Object, options);
    }

    [Fact]
    public async Task GetAll_NoPageOrPageSize_UsesConfiguredDefaultPageSize()
    {
        _memberService
            .Setup(s => s.GetAllAsync(It.IsAny<MemberListQuery>()))
            .ReturnsAsync(new PagedResult<Member> { Items = new List<Member>(), TotalCount = 0 });

        var response = await _sut.GetAll(new MemberListFilterRequest());

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        var body = Assert.IsType<MemberListResponse>(ok.Value);
        Assert.Equal(1, body.Page);
        Assert.Equal(20, body.PageSize);
    }

    [Fact]
    public async Task GetAll_PageSizeAboveMax_ClampsToConfiguredMax()
    {
        _memberService
            .Setup(s => s.GetAllAsync(It.IsAny<MemberListQuery>()))
            .ReturnsAsync(new PagedResult<Member> { Items = new List<Member>(), TotalCount = 0 });

        var response = await _sut.GetAll(new MemberListFilterRequest { PageSize = 500 });

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        var body = Assert.IsType<MemberListResponse>(ok.Value);
        Assert.Equal(100, body.PageSize);
    }

    [Fact]
    public async Task GetAll_PageBelowOne_ClampsToOne()
    {
        _memberService
            .Setup(s => s.GetAllAsync(It.IsAny<MemberListQuery>()))
            .ReturnsAsync(new PagedResult<Member> { Items = new List<Member>(), TotalCount = 0 });

        var response = await _sut.GetAll(new MemberListFilterRequest { Page = 0 });

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        var body = Assert.IsType<MemberListResponse>(ok.Value);
        Assert.Equal(1, body.Page);
    }

    [Fact]
    public async Task GetAll_PassesFilterFieldsThroughToService()
    {
        var sportId = Guid.NewGuid();
        var joinedFrom = new DateOnly(2026, 1, 1);
        var joinedTo = new DateOnly(2026, 6, 1);
        MemberListQuery? captured = null;

        _memberService
            .Setup(s => s.GetAllAsync(It.IsAny<MemberListQuery>()))
            .Callback<MemberListQuery>(q => captured = q)
            .ReturnsAsync(new PagedResult<Member> { Items = new List<Member>(), TotalCount = 0 });

        await _sut.GetAll(new MemberListFilterRequest
        {
            SportId = sportId,
            IsActive = false,
            Name = "ada",
            JoinedFrom = joinedFrom,
            JoinedTo = joinedTo,
            Page = 3,
            PageSize = 15
        });

        Assert.NotNull(captured);
        Assert.Equal(sportId, captured!.SportId);
        Assert.Equal(false, captured.IsActive);
        Assert.Equal("ada", captured.NameSearch);
        Assert.Equal(joinedFrom, captured.JoinedFrom);
        Assert.Equal(joinedTo, captured.JoinedTo);
        Assert.Equal(3, captured.Page);
        Assert.Equal(15, captured.PageSize);
    }

    [Fact]
    public async Task GetById_ReturnsOkWithMappedMemberResponse()
    {
        var member = new Member { Id = Guid.NewGuid(), FirstName = "Ada", LastName = "Lovelace", Email = "ada@example.com" };
        _memberService.Setup(s => s.GetByIdAsync(member.Id)).ReturnsAsync(member);

        var response = await _sut.GetById(member.Id);

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        var body = Assert.IsType<MemberResponse>(ok.Value);
        Assert.Equal(member.Id, body.Id);
        Assert.Equal("Ada", body.FirstName);
    }

    [Fact]
    public async Task Create_ValidRequest_ReturnsCreatedAtActionWithMappedMember()
    {
        var request = new CreateMemberRequest
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            Email = "ada@example.com",
            SportIds = new List<Guid>()
        };
        var created = new Member { Id = Guid.NewGuid(), FirstName = "Ada", LastName = "Lovelace", Email = "ada@example.com" };

        MemberCreateInput? captured = null;
        _memberService
            .Setup(s => s.CreateAsync(It.IsAny<MemberCreateInput>()))
            .Callback<MemberCreateInput>(i => captured = i)
            .ReturnsAsync(created);

        var response = await _sut.Create(request);

        var createdAt = Assert.IsType<CreatedAtActionResult>(response.Result);
        Assert.Equal(nameof(MembersController.GetById), createdAt.ActionName);
        var body = Assert.IsType<MemberResponse>(createdAt.Value);
        Assert.Equal(created.Id, body.Id);
        Assert.NotNull(captured);
        Assert.Equal(MemberRole.Member, captured!.Role);
    }

    [Fact]
    public async Task Create_RoleProvided_PassesThroughRoleUnchanged()
    {
        var request = new CreateMemberRequest
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            Email = "ada@example.com",
            Role = MemberRole.Coach
        };

        MemberCreateInput? captured = null;
        _memberService
            .Setup(s => s.CreateAsync(It.IsAny<MemberCreateInput>()))
            .Callback<MemberCreateInput>(i => captured = i)
            .ReturnsAsync(new Member { Id = Guid.NewGuid() });

        await _sut.Create(request);

        Assert.Equal(MemberRole.Coach, captured!.Role);
    }

    [Fact]
    public async Task Update_CallsServiceWithMappedInputAndReturnsNoContent()
    {
        var id = Guid.NewGuid();
        var request = new UpdateMemberRequest
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            Email = "ada@example.com",
            Role = MemberRole.Admin,
            IsActive = false
        };

        MemberUpdateInput? captured = null;
        _memberService
            .Setup(s => s.UpdateAsync(id, It.IsAny<MemberUpdateInput>()))
            .Callback<Guid, MemberUpdateInput>((_, i) => captured = i)
            .Returns(Task.CompletedTask);

        var result = await _sut.Update(id, request);

        Assert.IsType<NoContentResult>(result);
        Assert.NotNull(captured);
        Assert.Equal(MemberRole.Admin, captured!.Role);
        Assert.False(captured.IsActive);
    }

    [Fact]
    public async Task Delete_CallsServiceAndReturnsNoContent()
    {
        var id = Guid.NewGuid();
        _memberService.Setup(s => s.DeleteAsync(id)).Returns(Task.CompletedTask);

        var result = await _sut.Delete(id);

        Assert.IsType<NoContentResult>(result);
        _memberService.Verify(s => s.DeleteAsync(id), Times.Once);
    }
}
