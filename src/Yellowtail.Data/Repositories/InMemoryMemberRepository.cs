using System.Collections.Concurrent;
using Yellowtail.Data.Entities;

namespace Yellowtail.Data.Repositories;

public class InMemoryMemberRepository : IMemberRepository
{
    private readonly ConcurrentDictionary<Guid, Member> _members = new();

    public Task<IReadOnlyList<Member>> GetAllAsync()
    {
        IReadOnlyList<Member> members = _members.Values.ToList();
        return Task.FromResult(members);
    }

    public Task<Member?> GetByIdAsync(Guid id)
    {
        _members.TryGetValue(id, out var member);
        return Task.FromResult(member);
    }

    public Task AddAsync(Member member)
    {
        _members[member.Id] = member;
        return Task.CompletedTask;
    }

    public Task<bool> UpdateAsync(Member member)
    {
        if (!_members.ContainsKey(member.Id))
        {
            return Task.FromResult(false);
        }

        _members[member.Id] = member;
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        return Task.FromResult(_members.TryRemove(id, out _));
    }
}
