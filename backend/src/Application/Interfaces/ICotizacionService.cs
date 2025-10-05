using Application.DTOs;

namespace Application.Interfaces
{
    public interface ICotizacionService
    {
        Task<IEnumerable<CotizacionDto>> GetAllAsync();
        Task<CotizacionDto?> GetByIdAsync(int id);
        Task<CotizacionDto> CreateAsync(CreateCotizacionDto createDto, int usuarioId);
        Task<CotizacionDto> UpdateAsync(int id, UpdateCotizacionDto updateDto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<CotizacionDto>> SearchAsync(CotizacionSearchDto searchDto);
        Task<CotizacionDto> UpdateEstadoAsync(int id, string estado);
        Task<IEnumerable<CotizacionDto>> GetByUsuarioIdAsync(int usuarioId);
        Task<bool> ExistsAsync(int id);
        Task<string> GenerateNumeroCotizacionAsync();
    }
}