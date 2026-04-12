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
        Task<ReclamoDto> CreateReclamoAsync(CreateReclamoDto request, string usuario = "Sistema");
        Task<ReclamoDto> UpdateReclamoAsync(int id, UpdateReclamoDto request, string usuario = "Sistema");
        Task<ReclamoDto> AsignarUsuarioAsync(int reclamoId, int usuarioId, string usuario = "Sistema");
        Task<ReclamoDto> CambiarEstadoAsync(int reclamoId, EstadoReclamo nuevoEstado, string usuario = "Sistema");
        Task<ReclamoDto> ResolverReclamoAsync(int reclamoId, decimal? montoAprobado, string observaciones, string usuario = "Sistema");
        Task DeleteReclamoAsync(int id);

        // ── Historial ─────────────────────────────────────────────────────────
        Task<IEnumerable<ReclamoHistorialEntryDto>> GetHistorialAsync(int reclamoId);

        // ── Documentos ────────────────────────────────────────────────────────
        Task<IEnumerable<ReclamoDocumentoDto>> GetDocumentosAsync(int reclamoId);
        Task<ReclamoDocumentoDto> AddDocumentoMetadataAsync(
            int reclamoId,
            string docId,
            string nombre,
            long tamano,
            string tipoContenido,
            string ruta,
            string usuario);
        /// <summary>
        /// Removes the document entry from the JSON and returns its physical <c>Ruta</c>
        /// so the controller can delete the file.  Returns <c>null</c> if not found.
        /// </summary>
        Task<string?> RemoveDocumentoAsync(int reclamoId, string docId, string usuario = "Sistema");
        /// <summary>
        /// Returns the physical storage path for the given document — used by the download endpoint.
        /// Returns <c>null</c> if the document does not exist.
        /// </summary>
        Task<ReclamoDocumentoMetadata?> GetDocumentoMetadataAsync(int reclamoId, string docId);
    }
}
