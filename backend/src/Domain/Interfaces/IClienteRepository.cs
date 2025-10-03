using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IClienteRepository : IRepository<Cliente>
    {
        Task<Cliente?> GetByCodigoAsync(string codigo);
        Task<Cliente?> GetByNITAsync(string nit);
        Task<IEnumerable<Cliente>> GetByPerfilIdAsync(int perfilId);
        Task<IEnumerable<Cliente>> GetActivosAsync();
    }
}