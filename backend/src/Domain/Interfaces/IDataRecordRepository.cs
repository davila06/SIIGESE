using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IDataRecordRepository : IRepository<DataRecord>
    {
        Task<IEnumerable<DataRecord>> GetByUserIdAsync(int userId);
        Task<IEnumerable<DataRecord>> GetByStatusAsync(string status);
        Task<IEnumerable<DataRecord>> GetByPerfilIdAsync(int perfilId);
    }
}