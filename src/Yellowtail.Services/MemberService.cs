using Yellowtail.Data.Entities;
using Yellowtail.Data.Repositories;

namespace Yellowtail.Services;

public class MemberService(IMemberRepository repository) : IMemberService
{
    public Task<IReadOnlyList<Member>> GetAllAsync() => repository.GetAllAsync();

    public Task<Member?> GetByIdAsync(Guid id) => repository.GetByIdAsync(id);

    public async Task<Member> CreateAsync(string firstName, string lastName, string email)
    {
        var member = new Member
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            JoinedOn = DateOnly.FromDateTime(DateTime.UtcNow),
            IsActive = true
        };

        await repository.AddAsync(member);
        return member;
    }

    public async Task<bool> UpdateAsync(Guid id, string firstName, string lastName, string email, bool isActive)
    {
        var existing = await repository.GetByIdAsync(id);
        if (existing is null)
        {
            return false;
        }

        existing.FirstName = firstName;
        existing.LastName = lastName;
        existing.Email = email;
        existing.IsActive = isActive;

        return await repository.UpdateAsync(existing);
    }

    public Task<bool> DeleteAsync(Guid id) => repository.DeleteAsync(id);
}
