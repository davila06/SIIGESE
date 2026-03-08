using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;
using Application.DTOs;

namespace Application.Interfaces
{
    public interface ICobrosService
    {
        Task<IEnumerable<CobroDto>> GetAllCobrosAsync();
        Task<CobroDto> GetCobroByIdAsync(int id);
        Task<CobroDto> GetCobroByNumeroReciboAsync(string numeroRecibo);
        Task<IEnumerable<CobroDto>> GetCobrosByPolizaIdAsync(int polizaId);
        Task<IEnumerable<CobroDto>> GetCobrosByEstadoAsync(EstadoCobro estado);
        Task<IEnumerable<CobroDto>> GetCobrosVencidosAsync();
        Task<IEnumerable<CobroDto>> GetCobrosProximosVencerAsync(int dias);
        Task<IEnumerable<CobroDto>> GetCobrosProximosPorPeriodicidadAsync();
        Task<CobroStatsDto> GetCobrosStatsAsync();
        Task<CobroDto> CreateCobroAsync(CobroRequestDto request);
        Task<CobroDto> UpdateCobroAsync(int id, ActualizarCobroDto request);
        Task<CobroDto> RegistrarCobroAsync(RegistrarCobroRequestDto request);
        Task<string> GenerateNumeroReciboAsync();
        Task DeleteCobroAsync(int id);
        Task<CobroDto> CancelarCobroAsync(int id, string? motivo = null);
        Task<GenerarCobrosResultDto> GenerarCobrosAutomaticosAsync(int mesesAdelante = 3);
        Task<GenerarCobrosResultDto> GenerarCobrosPorPolizaAsync(int polizaId, int mesesAdelante = 3);
    }
}
