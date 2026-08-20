using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Yellowtail.Data.Entities;
using Yellowtail.Data.Enums;
using Yellowtail.Data.Repositories;
using Yellowtail.Services.Exceptions;
using Yellowtail.Services.Implementation;
using Yellowtail.Services.Models;

namespace Yellowtail.Tests.Unit.Services;

public class MemberServiceTests
{
    private readonly Mock<IMemberRepository> _repository = new();
    private readonly MemberService _sut;

    public MemberServiceTests()
    {
        _sut = new MemberService(_repository.Object, NullLogger<MemberService>.Instance);
    }

    [Fact]
    public async Task GetAllAsync_MapsQueryFieldsAndReturnsRepositoryResult()
    {
        var query = new MemberListQuery
        {
            SportId = Guid.NewGuid(),
            IsActive = false,
            NameSearch = "ada",
            JoinedFrom = new DateOnly(2026, 1, 1),
            JoinedTo = new DateOnly(2026, 12, 31),
            Page = 2,
            PageSize = 10
        };

        var expected = new PagedResult<Member> { Items = new List<Member> { new() }, TotalCount = 1 };
        MemberQuery? capturedQuery = null;
        _repository
            .Setup(r => r.GetAllAsync(It.IsAny<MemberQuery>()))
            .Callback<MemberQuery>(q => capturedQuery = q)
            .ReturnsAsync(expected);

        var result = await _sut.GetAllAsync(query);

        Assert.Same(expected, result);
        Assert.NotNull(capturedQuery);
        Assert.Equal(query.SportId, capturedQuery!.SportId);
        Assert.Equal(query.IsActive, capturedQuery.IsActive);
        Assert.Equal(query.NameSearch, capturedQuery.NameSearch);
        Assert.Equal(query.JoinedFrom, capturedQuery.JoinedFrom);
        Assert.Equal(query.JoinedTo, capturedQuery.JoinedTo);
        Assert.Equal(query.Page, capturedQuery.Page);
        Assert.Equal(query.PageSize, capturedQuery.PageSize);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingMember_ReturnsMember()
    {
        var member = new Member { Id = Guid.NewGuid() };
        _repository.Setup(r => r.GetByIdAsync(member.Id)).ReturnsAsync(member);

        var result = await _sut.GetByIdAsync(member.Id);

        Assert.Same(member, result);
    }

    [Fact]
    public async Task GetByIdAsync_MissingMember_ThrowsNotFoundException()
    {
        var id = Guid.NewGuid();
        _repository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Member?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(id));
    }

    [Fact]
    public async Task CreateAsync_ValidInput_CreatesMemberWithGeneratedIdAndTodayAsJoinedOn()
    {
        var input = new MemberCreateInput
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            Email = "ada@example.com",
            Role = MemberRole.Member,
            SportIds = new List<Guid>()
        };

        Member? added = null;
        _repository.Setup(r => r.AddAsync(It.IsAny<Member>()))
            .Callback<Member>(m => added = m)
            .Returns(Task.CompletedTask);
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(() => added);

        var result = await _sut.CreateAsync(input);

        Assert.NotNull(added);
        Assert.NotEqual(Guid.Empty, added!.Id);
        Assert.Equal("Ada", added.FirstName);
        Assert.True(added.IsActive);
        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), added.JoinedOn);
        Assert.Same(added, result);
        _repository.Verify(r => r.ReplaceMemberSportsAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithSportIds_ValidatesAndReplacesMemberSports()
    {
        var sportId = Guid.NewGuid();
        var input = new MemberCreateInput
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            Email = "ada@example.com",
            SportIds = new List<Guid> { sportId }
        };

        _repository.Setup(r => r.SportExistsAsync(sportId)).ReturnsAsync(true);
        _repository.Setup(r => r.AddAsync(It.IsAny<Member>())).Returns(Task.CompletedTask);
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Member?)null);

        await _sut.CreateAsync(input);

        _repository.Verify(
            r => r.ReplaceMemberSportsAsync(It.IsAny<Guid>(), It.Is<IEnumerable<Guid>>(ids => ids.Contains(sportId))),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_SportDoesNotExist_ThrowsValidationFailedExceptionAndDoesNotAddMember()
    {
        var sportId = Guid.NewGuid();
        var input = new MemberCreateInput
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            Email = "ada@example.com",
            SportIds = new List<Guid> { sportId }
        };

        _repository.Setup(r => r.SportExistsAsync(sportId)).ReturnsAsync(false);

        await Assert.ThrowsAsync<ValidationFailedException>(() => _sut.CreateAsync(input));
        _repository.Verify(r => r.AddAsync(It.IsAny<Member>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ExistingMember_UpdatesFieldsAndReplacesSports()
    {
        var id = Guid.NewGuid();
        var existing = new Member { Id = id, FirstName = "Old", IsActive = true };
        var input = new MemberUpdateInput
        {
            FirstName = "New",
            LastName = "Name",
            Email = "new@example.com",
            Role = MemberRole.Coach,
            IsActive = false,
            SportIds = new List<Guid>()
        };

        _repository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existing);
        _repository.Setup(r => r.UpdateAsync(existing)).ReturnsAsync(true);

        await _sut.UpdateAsync(id, input);

        Assert.Equal("New", existing.FirstName);
        Assert.Equal(MemberRole.Coach, existing.Role);
        Assert.False(existing.IsActive);
        _repository.Verify(r => r.UpdateAsync(existing), Times.Once);
        _repository.Verify(r => r.ReplaceMemberSportsAsync(id, input.SportIds), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_MissingMember_ThrowsNotFoundException()
    {
        var id = Guid.NewGuid();
        _repository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Member?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.UpdateAsync(id, new MemberUpdateInput { FirstName = "X", LastName = "Y", Email = "x@example.com" }));
    }

    [Fact]
    public async Task UpdateAsync_InvalidSportId_ThrowsValidationFailedException()
    {
        var id = Guid.NewGuid();
        var sportId = Guid.NewGuid();
        _repository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(new Member { Id = id });
        _repository.Setup(r => r.SportExistsAsync(sportId)).ReturnsAsync(false);

        await Assert.ThrowsAsync<ValidationFailedException>(() =>
            _sut.UpdateAsync(id, new MemberUpdateInput
            {
                FirstName = "X",
                LastName = "Y",
                Email = "x@example.com",
                SportIds = new List<Guid> { sportId }
            }));
    }

    [Fact]
    public async Task DeleteAsync_ExistingMember_CallsSoftDelete()
    {
        var id = Guid.NewGuid();
        _repository.Setup(r => r.SoftDeleteAsync(id)).ReturnsAsync(true);

        await _sut.DeleteAsync(id);

        _repository.Verify(r => r.SoftDeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_MissingMember_ThrowsNotFoundException()
    {
        var id = Guid.NewGuid();
        _repository.Setup(r => r.SoftDeleteAsync(id)).ReturnsAsync(false);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteAsync(id));
    }
}
