using Yellowtail.Data.Entities;

namespace Yellowtail.API.Contracts;

public record MemberResponse(Guid Id, string FirstName, string LastName, string Email, DateOnly JoinedOn, bool IsActive)
{
    public static MemberResponse FromEntity(Member member) =>
        new(member.Id, member.FirstName, member.LastName, member.Email, member.JoinedOn, member.IsActive);
}

public record CreateMemberRequest(string FirstName, string LastName, string Email);

public record UpdateMemberRequest(string FirstName, string LastName, string Email, bool IsActive);
