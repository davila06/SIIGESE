using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using System.Security.Claims;
using System.IO;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReclamosController : ControllerBase
    {
        private readonly IReclamoService _reclamoService;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ReclamosController> _logger;

        /// <summary>Returns the authenticated user's email or "Sistema" as fallback.</summary>
        private string CurrentUser =>
            User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue(ClaimTypes.Name)
            ?? User.FindFirstValue("sub")
            ?? "Sistema";

        public ReclamosController(
            IReclamoService reclamoService,
            IBlobStorageService blobStorageService,
            IConfiguration configuration,
            ILogger<ReclamosController> logger)
        {
            _reclamoService = reclamoService;
            _blobStorageService = blobStorageService;
            _configuration  = configuration;
            _logger         = logger;
        }

        /// <summary>
        /// Obtener todos los reclamos
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReclamoDto>>> GetAll()
        {
            try
            {
                var reclamos = await _reclamoService.GetAllReclamosAsync();
                return Ok(reclamos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los reclamos");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtener un reclamo por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ReclamoDto>> GetById(int id)
        {
            try
            {
                var reclamo = await _reclamoService.GetReclamoByIdAsync(id);
                if (reclamo == null)
                    return NotFound($"Reclamo con ID {id} no encontrado");

                return Ok(reclamo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el reclamo {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Crear un nuevo reclamo
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ReclamoDto>> Create([FromBody] CreateReclamoDto createReclamoDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var reclamo = await _reclamoService.CreateReclamoAsync(createReclamoDto);
                return CreatedAtAction(nameof(GetById), new { id = reclamo.Id }, reclamo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el reclamo");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Actualizar un reclamo existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ReclamoDto>> Update(int id, [FromBody] UpdateReclamoDto updateReclamoDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var reclamo = await _reclamoService.UpdateReclamoAsync(id, updateReclamoDto);
                if (reclamo == null)
                    return NotFound($"Reclamo con ID {id} no encontrado");

                return Ok(reclamo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el reclamo {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Eliminar un reclamo
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                await _reclamoService.DeleteReclamoAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el reclamo {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtener estadísticas de reclamos
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<ReclamoStatsDto>> GetStats()
        {
            try
            {
                var stats = await _reclamoService.GetReclamosStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de reclamos");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtener reclamos por póliza
        /// </summary>
        [HttpGet("poliza/{numeroPoliza}")]
        public async Task<ActionResult<IEnumerable<ReclamoDto>>> GetByPoliza(string numeroPoliza)
        {
            try
            {
                var reclamos = await _reclamoService.GetReclamosByPolizaAsync(numeroPoliza);
                return Ok(reclamos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reclamos de la póliza {NumeroPoliza}", numeroPoliza);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtener reclamos por estado
        /// </summary>
        [HttpGet("estado/{estado}")]
        public async Task<ActionResult<IEnumerable<ReclamoDto>>> GetByEstado(int estado)
        {
            try
            {
                if (!Enum.IsDefined(typeof(EstadoReclamo), estado))
                    return BadRequest("Estado de reclamo inválido");

                var estadoReclamo = (EstadoReclamo)estado;
                var reclamos = await _reclamoService.GetReclamosByEstadoAsync(estadoReclamo);
                return Ok(reclamos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reclamos por estado {Estado}", estado);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtener reclamos vencidos
        /// </summary>
        [HttpGet("vencidos")]
        public async Task<ActionResult<IEnumerable<ReclamoDto>>> GetVencidos()
        {
            try
            {
                var reclamos = await _reclamoService.GetReclamosVencidosAsync();
                return Ok(reclamos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reclamos vencidos");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtener reclamos con filtros (paginado)
        /// </summary>
        [HttpPost("filtrar")]
        public async Task<ActionResult<PagedResultDto<ReclamoDto>>> GetByFiltro([FromBody] ReclamoFilterDto filtro)
        {
            try
            {
                var result = await _reclamoService.GetReclamosByFiltroAsync(filtro);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al filtrar reclamos");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Asignar usuario a un reclamo
        /// </summary>
        [HttpPatch("{id}/asignar/{usuarioId}")]
        public async Task<ActionResult<ReclamoDto>> AsignarUsuario(int id, int usuarioId)
        {
            try
            {
                var reclamo = await _reclamoService.AsignarUsuarioAsync(id, usuarioId, CurrentUser);
                if (reclamo == null)
                    return NotFound($"Reclamo con ID {id} no encontrado");

                return Ok(reclamo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar usuario {UsuarioId} al reclamo {Id}", usuarioId, id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Cambiar estado de un reclamo
        /// </summary>
        [HttpPatch("{id}/estado/{estado}")]
        public async Task<ActionResult<ReclamoDto>> CambiarEstado(int id, int estado)
        {
            try
            {
                if (!Enum.IsDefined(typeof(EstadoReclamo), estado))
                    return BadRequest("Estado de reclamo inválido");

                var estadoReclamo = (EstadoReclamo)estado;
                var reclamo = await _reclamoService.CambiarEstadoAsync(id, estadoReclamo, CurrentUser);
                if (reclamo == null)
                    return NotFound($"Reclamo con ID {id} no encontrado");

                return Ok(reclamo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado del reclamo {Id} a {Estado}", id, estado);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Resolver un reclamo
        /// </summary>
        [HttpPatch("{id}/resolver")]
        public async Task<ActionResult<ReclamoDto>> Resolver(int id, [FromBody] ResolverReclamoDto resolverDto)
        {
            try
            {
                var reclamo = await _reclamoService.ResolverReclamoAsync(id, resolverDto.MontoAprobado, resolverDto.Observaciones ?? string.Empty, CurrentUser);
                if (reclamo == null)
                    return NotFound($"Reclamo con ID {id} no encontrado");

                return Ok(reclamo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al resolver el reclamo {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene el historial de eventos del reclamo.
        /// </summary>
        [HttpGet("{id}/historial")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ReclamoHistorialEntryDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ReclamoHistorialEntryDto>>> GetHistorial(int id)
        {
            try
            {
                var reclamo = await _reclamoService.GetReclamoByIdAsync(id);
                if (reclamo == null)
                    return NotFound($"Reclamo con ID {id} no encontrado");

                var historial = await _reclamoService.GetHistorialAsync(id);
                return Ok(historial);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener historial del reclamo {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Lista los documentos adjuntos del reclamo.
        /// </summary>
        [HttpGet("{id}/documentos")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ReclamoDocumentoDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ReclamoDocumentoDto>>> GetDocumentos(int id)
        {
            try
            {
                var reclamo = await _reclamoService.GetReclamoByIdAsync(id);
                if (reclamo == null)
                    return NotFound($"Reclamo con ID {id} no encontrado");

                var documentos = await _reclamoService.GetDocumentosAsync(id);
                return Ok(documentos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener documentos del reclamo {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Sube y registra un documento adjunto para el reclamo.
        /// </summary>
        [HttpPost("{id}/documentos")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ReclamoDocumentoDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ReclamoDocumentoDto>> UploadDocumento(int id, [FromForm] IFormFile file)
        {
            try
            {
                var reclamo = await _reclamoService.GetReclamoByIdAsync(id);
                if (reclamo == null)
                    return NotFound($"Reclamo con ID {id} no encontrado");

                if (file == null || file.Length == 0)
                    return BadRequest("Archivo no proporcionado");

                var maxFileSizeInMb = _configuration.GetValue<int?>("FileUpload:Reclamos:MaxFileSizeInMB")
                    ?? _configuration.GetValue<int?>("FileUpload:MaxFileSizeInMB")
                    ?? 15;
                var maxFileSizeBytes = maxFileSizeInMb * 1024L * 1024L;
                if (file.Length > maxFileSizeBytes)
                    return BadRequest($"El archivo excede el límite de {maxFileSizeInMb} MB");

                var allowedExtensions = _configuration
                    .GetSection("FileUpload:Reclamos:AllowedExtensions")
                    .Get<string[]>()
                    ?? new[] { ".pdf", ".png", ".jpg", ".jpeg", ".doc", ".docx", ".xls", ".xlsx" };

                var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? string.Empty;
                if (!allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
                    return BadRequest($"Extensión de archivo no permitida: {extension}");

                var docId = Guid.NewGuid().ToString("N");
                var safeFileName = SanitizeFileName(file.FileName);
                var storedFileName = $"{docId}_{safeFileName}";
                var blobPrefix = _configuration.GetValue<string>("AzureBlobStorage:ReclamosPrefix") ?? "reclamos";
                var normalizedPrefix = blobPrefix.Trim('/');
                var blobPath = $"{normalizedPrefix}/{id}/{storedFileName}";

                await using (var stream = file.OpenReadStream())
                {
                    await _blobStorageService.UploadAsync(
                        blobPath,
                        stream,
                        file.ContentType ?? "application/octet-stream",
                        HttpContext.RequestAborted);
                }

                var documento = await _reclamoService.AddDocumentoMetadataAsync(
                    id,
                    docId,
                    safeFileName,
                    file.Length,
                    file.ContentType ?? "application/octet-stream",
                    blobPath,
                    CurrentUser);

                return CreatedAtAction(nameof(GetDocumentos), new { id }, documento);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir documento para reclamo {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Elimina un documento adjunto del reclamo.
        /// </summary>
        [HttpDelete("{id}/documentos/{docId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteDocumento(int id, string docId)
        {
            try
            {
                var reclamo = await _reclamoService.GetReclamoByIdAsync(id);
                if (reclamo == null)
                    return NotFound($"Reclamo con ID {id} no encontrado");

                var deletedPath = await _reclamoService.RemoveDocumentoAsync(id, docId, CurrentUser);
                if (string.IsNullOrWhiteSpace(deletedPath))
                    return NotFound($"Documento con ID {docId} no encontrado en el reclamo {id}");

                await _blobStorageService.DeleteAsync(deletedPath, HttpContext.RequestAborted);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar documento {DocId} del reclamo {Id}", docId, id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        private static string SanitizeFileName(string fileName)
        {
            var onlyFileName = Path.GetFileName(fileName);
            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                onlyFileName = onlyFileName.Replace(invalidChar, '_');
            }

            return string.IsNullOrWhiteSpace(onlyFileName) ? "archivo" : onlyFileName;
        }
    }

    public class ResolverReclamoDto
    {
        public decimal? MontoAprobado { get; set; }
        public string? Observaciones { get; set; }
    }

    public class AsignarReclamoDto
    {
        public int UsuarioId { get; set; }
    }
}