using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IPasswordResetTokenRepository : IRepository<PasswordResetToken>
    {
        Task<PasswordResetToken?> GetByTokenAsync(string token);
        Task<PasswordResetToken?> GetActiveTokenByUserIdAsync(int userId);
        Task InvalidateUserTokensAsync(int userId);
        Task CleanupExpiredTokensAsync();
    }
}