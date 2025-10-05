using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IReclamoRepository : IRepository<Reclamo>
    {
        Task<IEnumerable<Reclamo>> GetReclamosByPolizaIdAsync(int polizaId);
        Task<IEnumerable<Reclamo>> GetReclamosByEstadoAsync(EstadoReclamo estado);
        Task<IEnumerable<Reclamo>> GetReclamosByTipoAsync(TipoReclamo tipo);
        Task<IEnumerable<Reclamo>> GetReclamosByPrioridadAsync(PrioridadReclamo prioridad);
        Task<IEnumerable<Reclamo>> GetReclamosByUsuarioAsignadoAsync(int usuarioId);
        Task<IEnumerable<Reclamo>> GetReclamosVencidosAsync();
        Task<string> GenerateNumeroReclamoAsync();
    }
}