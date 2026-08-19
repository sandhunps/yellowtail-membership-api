using Yellowtail.Data.Entities;

namespace Yellowtail.Data.Repositories;

public interface IMemberRepository
{
    Task<IReadOnlyList<Member>> GetAllAsync();
    Task<Member?> GetByIdAsync(Guid id);
    Task AddAsync(Member member);
    Task<bool> UpdateAsync(Member member);
    Task<bool> DeleteAsync(Guid id);
}
