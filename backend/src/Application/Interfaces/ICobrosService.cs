using Application.DTOs;

namespace Application.Interfaces
{
    public interface ICobrosService
    {
        Task<IEnumerable<CobroDto>> GetAllCobrosAsync();
        Task<CobroDto?> GetCobroByIdAsync(int id);
        Task<CobroDto?> GetCobroByNumeroReciboAsync(string numeroRecibo);
        Task<IEnumerable<CobroDto>> GetCobrosByPolizaIdAsync(int polizaId);
        Task<IEnumerable<CobroDto>> GetCobrosByEstadoAsync(string estado);
        Task<IEnumerable<CobroDto>> GetCobrosVencidosAsync();
        Task<IEnumerable<CobroDto>> GetCobrosProximosVencerAsync(int dias = 7);
        Task<CobroStatsDto> GetCobrosStatsAsync();
        Task<CobroDto> CreateCobroAsync(CobroRequestDto request);
        Task<CobroDto?> UpdateCobroAsync(int id, ActualizarCobroDto request);
        Task<CobroDto?> RegistrarCobroAsync(RegistrarCobroRequestDto request);
        Task<bool> DeleteCobroAsync(int id);
        Task<string> GenerateNumeroReciboAsync();
    }
}