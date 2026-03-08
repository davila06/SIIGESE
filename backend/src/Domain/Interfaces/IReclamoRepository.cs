using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Queries;

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

        // SQL-level filtering with pagination — avoids loading the full table
        Task<(IEnumerable<Reclamo> Items, int TotalCount)> GetByFiltroAsync(ReclamoQueryFilter filtro);

        // Aggregate queries for stats — one DB round-trip each
        Task<int> GetTotalCountAsync();
        Task<int> GetCountByEstadoAsync(EstadoReclamo estado);
        Task<decimal> GetMontoTotalReclamadoAsync();
        Task<decimal> GetMontoTotalAprobadoAsync();

        Task<string> GenerateNumeroReclamoAsync();
        Task<Reclamo> AddAsync(Reclamo reclamo);
        Task UpdateAsync(Reclamo reclamo);
        Task DeleteAsync(int id);
    }
}
