using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IRoleRepository : IRepository<Role>
    {
        Task<Role?> GetByNameAsync(string name);
        Task<IEnumerable<Role>> GetActiveRolesAsync();
    }
}