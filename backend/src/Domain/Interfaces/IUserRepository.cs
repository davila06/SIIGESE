using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUserNameAsync(string userName);
        Task<IEnumerable<User>> GetUsersWithRolesAsync();
        Task<User?> GetUserWithRolesAsync(int userId);
        Task AssignRoleToUserAsync(int userId, int roleId);
        Task ClearUserRolesAsync(int userId);
    }
}