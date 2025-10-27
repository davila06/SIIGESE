using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReclamosController : ControllerBase
    {
        private readonly IReclamoService _reclamoService;
        private readonly ILogger<ReclamosController> _logger;

        public ReclamosController(IReclamoService reclamoService, ILogger<ReclamosController> logger)
        {
            _reclamoService = reclamoService;
            _logger = logger;
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
                var reclamos = await _reclamoService.GetReclamosByPolizaIdAsync(numeroPoliza);
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
        /// Obtener reclamos con filtros
        /// </summary>
        [HttpPost("filtrar")]
        public async Task<ActionResult<IEnumerable<ReclamoDto>>> GetByFiltro([FromBody] ReclamoFilterDto filtro)
        {
            try
            {
                var reclamos = await _reclamoService.GetReclamosByFiltroAsync(filtro);
                return Ok(reclamos);
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
                var reclamo = await _reclamoService.AsignarUsuarioAsync(id, usuarioId);
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
                var reclamo = await _reclamoService.CambiarEstadoAsync(id, estadoReclamo);
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
                var reclamo = await _reclamoService.ResolverReclamoAsync(id, resolverDto.MontoAprobado, resolverDto.Observaciones ?? string.Empty);
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
    }

    public class ResolverReclamoDto
    {
        public decimal? MontoAprobado { get; set; }
        public string? Observaciones { get; set; }
    }
}