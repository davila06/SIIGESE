using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IEmailLogRepository : IRepository<EmailLog>
    {
        Task<IEnumerable<EmailLog>> GetPagedAsync(int pageNumber, int pageSize);
        Task<int> GetSuccessCountAsync();
        Task<int> GetFailedCountAsync();
    }
}