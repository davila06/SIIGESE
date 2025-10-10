using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories
{
    public class EmailConfigRepository : IEmailConfigRepository
    {
        private readonly ApplicationDbContext _context;

        public EmailConfigRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EmailConfig>> GetAllAsync()
        {
            return await _context.EmailConfigs
                .OrderByDescending(x => x.IsDefault)
                .ThenBy(x => x.ConfigName)
                .ToListAsync();
        }

        public async Task<EmailConfig?> GetByIdAsync(int id)
        {
            return await _context.EmailConfigs.FindAsync(id);
        }

        public async Task<EmailConfig?> GetDefaultAsync()
        {
            return await _context.EmailConfigs
                .Where(x => x.IsDefault && x.IsActive)
                .FirstOrDefaultAsync();
        }

        public async Task<EmailConfig> CreateAsync(EmailConfig emailConfig)
        {
            emailConfig.CreatedAt = DateTime.UtcNow;
            emailConfig.UpdatedAt = DateTime.UtcNow;
            
            _context.EmailConfigs.Add(emailConfig);
            await _context.SaveChangesAsync();
            return emailConfig;
        }

        public async Task<EmailConfig> UpdateAsync(EmailConfig emailConfig)
        {
            emailConfig.UpdatedAt = DateTime.UtcNow;
            
            _context.EmailConfigs.Update(emailConfig);
            await _context.SaveChangesAsync();
            return emailConfig;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.EmailConfigs.FindAsync(id);
            if (entity == null) return false;

            _context.EmailConfigs.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetAsDefaultAsync(int id)
        {
            // Desactivar todas las configuraciones por defecto
            await _context.EmailConfigs
                .Where(x => x.IsDefault)
                .ExecuteUpdateAsync(x => x.SetProperty(p => p.IsDefault, false));

            // Activar la nueva configuración por defecto
            var rowsAffected = await _context.EmailConfigs
                .Where(x => x.Id == id)
                .ExecuteUpdateAsync(x => x
                    .SetProperty(p => p.IsDefault, true)
                    .SetProperty(p => p.UpdatedAt, DateTime.UtcNow));

            return rowsAffected > 0;
        }

        public async Task<bool> ToggleActiveStatusAsync(int id)
        {
            var entity = await _context.EmailConfigs.FindAsync(id);
            if (entity == null) return false;

            entity.IsActive = !entity.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsByNameAsync(string configName, int? excludeId = null)
        {
            return await _context.EmailConfigs
                .Where(x => x.ConfigName == configName && (excludeId == null || x.Id != excludeId))
                .AnyAsync();
        }
    }
}