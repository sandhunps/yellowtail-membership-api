using Yellowtail.Data.Entities;

namespace Yellowtail.Services;

public interface IMemberService
{
    Task<IReadOnlyList<Member>> GetAllAsync();
    Task<Member?> GetByIdAsync(Guid id);
    Task<Member> CreateAsync(string firstName, string lastName, string email);
    Task<bool> UpdateAsync(Guid id, string firstName, string lastName, string email, bool isActive);
    Task<bool> DeleteAsync(Guid id);
}
