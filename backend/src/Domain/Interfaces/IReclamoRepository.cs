using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IReclamoRepository
    {
        Task<IEnumerable<Reclamo>> GetAllAsync();
        Task<Reclamo?> GetByIdAsync(int id);
        Task<IEnumerable<Reclamo>> GetReclamosByPolizaIdAsync(string numeroPoliza);
        Task<IEnumerable<Reclamo>> GetReclamosByEstadoAsync(EstadoReclamo estado);
        Task<IEnumerable<Reclamo>> GetReclamosByTipoAsync(TipoReclamo tipo);
        Task<IEnumerable<Reclamo>> GetReclamosByPrioridadAsync(PrioridadReclamo prioridad);
        Task<IEnumerable<Reclamo>> GetReclamosByUsuarioAsignadoAsync(int usuarioId);
        Task<IEnumerable<Reclamo>> GetReclamosVencidosAsync();
        Task<string> GenerateNumeroReclamoAsync();
        Task<Reclamo> AddAsync(Reclamo reclamo);
        Task UpdateAsync(Reclamo reclamo);
        Task DeleteAsync(int id);
    }
}
