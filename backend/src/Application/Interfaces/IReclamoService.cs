using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IReclamoService
    {
        Task<IEnumerable<ReclamoDto>> GetAllReclamosAsync();
        Task<ReclamoDto?> GetReclamoByIdAsync(int id);
        Task<ReclamoDto> CreateReclamoAsync(CreateReclamoDto createReclamoDto);
        Task<ReclamoDto?> UpdateReclamoAsync(int id, UpdateReclamoDto updateReclamoDto);
        Task<bool> DeleteReclamoAsync(int id);
        Task<IEnumerable<ReclamoDto>> GetReclamosByPolizaIdAsync(int polizaId);
        Task<IEnumerable<ReclamoDto>> GetReclamosByEstadoAsync(EstadoReclamo estado);
        Task<IEnumerable<ReclamoDto>> GetReclamosByTipoAsync(TipoReclamo tipo);
        Task<IEnumerable<ReclamoDto>> GetReclamosByPrioridadAsync(PrioridadReclamo prioridad);
        Task<IEnumerable<ReclamoDto>> GetReclamosByUsuarioAsignadoAsync(int usuarioId);
        Task<IEnumerable<ReclamoDto>> GetReclamosVencidosAsync();
        Task<IEnumerable<ReclamoDto>> GetReclamosByFiltroAsync(ReclamoFilterDto filtro);
        Task<ReclamoStatsDto> GetReclamosStatsAsync();
        Task<ReclamoDto?> AsignarUsuarioAsync(int reclamoId, int usuarioId);
        Task<ReclamoDto?> CambiarEstadoAsync(int reclamoId, EstadoReclamo nuevoEstado);
        Task<ReclamoDto?> ResolverReclamoAsync(int reclamoId, decimal? montoAprobado, string? observaciones);
    }
}