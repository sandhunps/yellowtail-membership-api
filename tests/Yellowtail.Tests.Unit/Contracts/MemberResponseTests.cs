using Yellowtail.API.Contracts.Members;
using Yellowtail.Data.Entities;
using Yellowtail.Data.Enums;

namespace Yellowtail.Tests.Unit.Contracts;

public class MemberResponseTests
{
    [Fact]
    public void FromEntity_MapsAllFieldsIncludingSports()
    {
        var sport = new Sport { Id = Guid.NewGuid(), Name = "Tennis" };
        var member = new Member
        {
            Id = Guid.NewGuid(),
            FirstName = "Ada",
            LastName = "Lovelace",
            Email = "ada@example.com",
            Phone = "12345",
            DateOfBirth = new DateOnly(1990, 1, 1),
            PhotoUrl = "https://example.com/a.jpg",
            Role = MemberRole.Coach,
            IsActive = true,
            JoinedOn = new DateOnly(2026, 1, 1),
            MemberSports = new List<MemberSport> { new() { Sport = sport, SportId = sport.Id } }
        };

        var response = MemberResponse.FromEntity(member);

        Assert.Equal(member.Id, response.Id);
        Assert.Equal("Ada", response.FirstName);
        Assert.Equal("Lovelace", response.LastName);
        Assert.Equal("ada@example.com", response.Email);
        Assert.Equal("12345", response.Phone);
        Assert.Equal(new DateOnly(1990, 1, 1), response.DateOfBirth);
        Assert.Equal("https://example.com/a.jpg", response.PhotoUrl);
        Assert.Equal(MemberRole.Coach, response.Role);
        Assert.True(response.IsActive);
        Assert.Equal(new DateOnly(2026, 1, 1), response.JoinedOn);
        Assert.Single(response.Sports);
        Assert.Equal("Tennis", response.Sports[0].Name);
    }

    [Fact]
    public void FromEntity_NoSports_ReturnsEmptySportsList()
    {
        var member = new Member { Id = Guid.NewGuid(), FirstName = "Ada", LastName = "Lovelace", Email = "ada@example.com" };

        var response = MemberResponse.FromEntity(member);

        Assert.Empty(response.Sports);
    }
}
