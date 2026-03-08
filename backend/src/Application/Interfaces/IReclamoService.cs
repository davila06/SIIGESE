using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;
using Application.DTOs;

namespace Application.Interfaces
{
    public interface IReclamoService
    {
        Task<IEnumerable<ReclamoDto>> GetAllReclamosAsync();
        Task<ReclamoDto> GetReclamoByIdAsync(int id);
        Task<IEnumerable<ReclamoDto>> GetReclamosByPolizaAsync(string numeroPoliza);
        Task<IEnumerable<ReclamoDto>> GetReclamosByEstadoAsync(EstadoReclamo estado);
        Task<IEnumerable<ReclamoDto>> GetReclamosVencidosAsync();
        Task<PagedResultDto<ReclamoDto>> GetReclamosByFiltroAsync(ReclamoFilterDto filtro);
        Task<ReclamoStatsDto> GetReclamosStatsAsync();
        Task<string> GenerateNumeroReclamoAsync();
        Task<ReclamoDto> CreateReclamoAsync(CreateReclamoDto request);
        Task<ReclamoDto> UpdateReclamoAsync(int id, UpdateReclamoDto request);
        Task<ReclamoDto> AsignarUsuarioAsync(int reclamoId, int usuarioId);
        Task<ReclamoDto> CambiarEstadoAsync(int reclamoId, EstadoReclamo nuevoEstado);
        Task<ReclamoDto> ResolverReclamoAsync(int reclamoId, decimal? montoAprobado, string observaciones);
        Task DeleteReclamoAsync(int id);
    }
}
