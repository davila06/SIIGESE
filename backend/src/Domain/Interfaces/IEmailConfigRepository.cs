using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IEmailConfigRepository
    {
        Task<IEnumerable<EmailConfig>> GetAllAsync();
        Task<EmailConfig?> GetByIdAsync(int id);
        Task<EmailConfig?> GetDefaultAsync();
        Task<EmailConfig> CreateAsync(EmailConfig emailConfig);
        Task<EmailConfig> UpdateAsync(EmailConfig emailConfig);
        Task<bool> DeleteAsync(int id);
        Task<bool> SetAsDefaultAsync(int id);
        Task<bool> ToggleActiveStatusAsync(int id);
        Task<bool> ExistsByNameAsync(string configName, int? excludeId = null);
    }
}