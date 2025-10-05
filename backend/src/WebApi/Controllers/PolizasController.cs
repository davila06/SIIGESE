using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Application.DTOs;
using Application.Interfaces;
using System.Security.Claims;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PolizasController : ControllerBase
    {
        private readonly IPolizaService _polizaService;
        private readonly ILogger<PolizasController> _logger;

        public PolizasController(IPolizaService polizaService, ILogger<PolizasController> logger)
        {
            _polizaService = polizaService;
            _logger = logger;
        }

        /// <summary>
        /// Obtener todas las pólizas activas
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PolizaDto>))]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var polizas = await _polizaService.GetAllAsync();
                return Ok(polizas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo pólizas");
                return BadRequest(new { message = "Error obteniendo pólizas" });
            }
        }

        /// <summary>
        /// Obtener pólizas por perfil
        /// </summary>
        [HttpGet("perfil/{perfilId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PolizaDto>))]
        public async Task<IActionResult> GetByPerfil(int perfilId)
        {
            try
            {
                var polizas = await _polizaService.GetByPerfilIdAsync(perfilId);
                return Ok(polizas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo pólizas por perfil {PerfilId}", perfilId);
                return BadRequest(new { message = "Error obteniendo pólizas" });
            }
        }

        /// <summary>
        /// Obtener póliza por ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PolizaDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var poliza = await _polizaService.GetByIdAsync(id);
                if (poliza == null)
                    return NotFound(new { message = "Póliza no encontrada" });

                return Ok(poliza);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo póliza {Id}", id);
                return BadRequest(new { message = "Error obteniendo póliza" });
            }
        }

        /// <summary>
        /// Buscar póliza por número
        /// </summary>
        [HttpGet("numero/{numeroPoliza}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PolizaDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByNumero(string numeroPoliza)
        {
            try
            {
                var poliza = await _polizaService.GetByNumeroPolizaAsync(numeroPoliza);
                if (poliza == null)
                    return NotFound(new { message = "Póliza no encontrada" });

                return Ok(poliza);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo póliza por número {NumeroPoliza}", numeroPoliza);
                return BadRequest(new { message = "Error obteniendo póliza" });
            }
        }

        /// <summary>
        /// Obtener pólizas por aseguradora
        /// </summary>
        [HttpGet("aseguradora/{aseguradora}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PolizaDto>))]
        public async Task<IActionResult> GetByAseguradora(string aseguradora)
        {
            try
            {
                var polizas = await _polizaService.GetByAseguradoraAsync(aseguradora);
                return Ok(polizas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo pólizas por aseguradora {Aseguradora}", aseguradora);
                return BadRequest(new { message = "Error obteniendo pólizas" });
            }
        }

        /// <summary>
        /// Crear nueva póliza
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,DataLoader")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(PolizaDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreatePolizaDto dto)
        {
            try
            {
                var poliza = await _polizaService.CreateAsync(dto);
                _logger.LogInformation("Póliza creada: {NumeroPoliza} por usuario {UserId}", dto.NumeroPoliza, GetCurrentUserId());
                return CreatedAtAction(nameof(GetById), new { id = poliza.Id }, poliza);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando póliza");
                return BadRequest(new { message = "Error creando póliza" });
            }
        }

        /// <summary>
        /// Actualizar póliza existente
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,DataLoader")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PolizaDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] CreatePolizaDto dto)
        {
            try
            {
                var poliza = await _polizaService.UpdateAsync(id, dto);
                _logger.LogInformation("Póliza {Id} actualizada por usuario {UserId}", id, GetCurrentUserId());
                return Ok(poliza);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Póliza no encontrada" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando póliza {Id}", id);
                return BadRequest(new { message = "Error actualizando póliza" });
            }
        }

        /// <summary>
        /// Eliminar póliza
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,DataLoader")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _polizaService.DeleteAsync(id);
                _logger.LogInformation("Póliza {Id} eliminada por usuario {UserId}", id, GetCurrentUserId());
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Póliza no encontrada" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando póliza {Id}", id);
                return BadRequest(new { message = "Error eliminando póliza" });
            }
        }

        /// <summary>
        /// Cargar pólizas desde archivo Excel
        /// </summary>
        [HttpPost("upload")]
        [Authorize(Roles = "Admin,DataLoader")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DataUploadResultDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadExcel([FromForm] int perfilId, [FromForm] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "Archivo no proporcionado" });

                // Validar extensión
                var allowedExtensions = new[] { ".xlsx", ".xls" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                    return BadRequest(new { message = "Solo se permiten archivos Excel (.xlsx, .xls)" });

                // Validar tamaño (10MB máximo)
                if (file.Length > 10 * 1024 * 1024)
                    return BadRequest(new { message = "El archivo excede el tamaño máximo de 10MB" });

                var userId = GetCurrentUserId();
                var result = await _polizaService.ProcesarExcelPolizasAsync(perfilId, file, int.Parse(userId));
                
                _logger.LogInformation("Archivo Excel de pólizas procesado: {FileName}, {ProcessedRecords} registros procesados por usuario {UserId}", 
                    file.FileName, result.ProcessedRecords, userId);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando archivo Excel de pólizas");
                return BadRequest(new { message = "Error procesando archivo Excel" });
            }
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0";
        }
    }
}