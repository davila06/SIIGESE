using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CotizacionesController : ControllerBase
    {
        private readonly ICotizacionService _cotizacionService;
        private readonly ILogger<CotizacionesController> _logger;

        public CotizacionesController(
            ICotizacionService cotizacionService,
            ILogger<CotizacionesController> logger)
        {
            _cotizacionService = cotizacionService;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out int userId) ? userId : 0;
        }

        /// <summary>
        /// Obtener todas las cotizaciones
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,DataLoader,Agent")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CotizacionDto>))]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var cotizaciones = await _cotizacionService.GetAllAsync();
                return Ok(cotizaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo cotizaciones");
                return BadRequest(new { message = "Error obteniendo cotizaciones" });
            }
        }

        /// <summary>
        /// Obtener cotización por ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,DataLoader,Agent")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CotizacionDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var cotizacion = await _cotizacionService.GetByIdAsync(id);
                if (cotizacion == null)
                {
                    return NotFound(new { message = "Cotización no encontrada" });
                }

                return Ok(cotizacion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo cotización {Id}", id);
                return BadRequest(new { message = "Error obteniendo cotización" });
            }
        }

        /// <summary>
        /// Crear nueva cotización
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,DataLoader,Agent")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CotizacionDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateCotizacionDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var usuarioId = GetCurrentUserId();
                var cotizacion = await _cotizacionService.CreateAsync(createDto, usuarioId);
                
                _logger.LogInformation("Cotización {NumeroCotizacion} creada por usuario {UserId}", 
                    cotizacion.NumeroCotizacion, usuarioId);

                return CreatedAtAction(nameof(GetById), new { id = cotizacion.Id }, cotizacion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando cotización");
                return BadRequest(new { message = "Error creando cotización" });
            }
        }

        /// <summary>
        /// Actualizar cotización existente
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,DataLoader,Agent")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CotizacionDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCotizacionDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var cotizacion = await _cotizacionService.UpdateAsync(id, updateDto);
                _logger.LogInformation("Cotización {Id} actualizada por usuario {UserId}", id, GetCurrentUserId());
                return Ok(cotizacion);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Cotización no encontrada" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando cotización {Id}", id);
                return BadRequest(new { message = "Error actualizando cotización" });
            }
        }

        /// <summary>
        /// Eliminar cotización
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,DataLoader")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _cotizacionService.DeleteAsync(id);
                _logger.LogInformation("Cotización {Id} eliminada por usuario {UserId}", id, GetCurrentUserId());
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Cotización no encontrada" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando cotización {Id}", id);
                return BadRequest(new { message = "Error eliminando cotización" });
            }
        }

        /// <summary>
        /// Buscar cotizaciones con filtros
        /// </summary>
        [HttpPost("search")]
        [Authorize(Roles = "Admin,DataLoader,Agent")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CotizacionDto>))]
        public async Task<IActionResult> Search([FromBody] CotizacionSearchDto searchDto)
        {
            try
            {
                var cotizaciones = await _cotizacionService.SearchAsync(searchDto);
                return Ok(cotizaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error buscando cotizaciones");
                return BadRequest(new { message = "Error buscando cotizaciones" });
            }
        }

        /// <summary>
        /// Actualizar estado de cotización
        /// </summary>
        [HttpPatch("{id}/estado")]
        [Authorize(Roles = "Admin,DataLoader,Agent")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CotizacionDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateEstado(int id, [FromBody] string estado)
        {
            try
            {
                var cotizacion = await _cotizacionService.UpdateEstadoAsync(id, estado);
                _logger.LogInformation("Estado de cotización {Id} actualizado a {Estado} por usuario {UserId}", 
                    id, estado, GetCurrentUserId());
                return Ok(cotizacion);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Cotización no encontrada" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando estado de cotización {Id}", id);
                return BadRequest(new { message = "Error actualizando estado de cotización" });
            }
        }

        /// <summary>
        /// Obtener cotizaciones del usuario actual
        /// </summary>
        [HttpGet("mis-cotizaciones")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CotizacionDto>))]
        public async Task<IActionResult> GetMisCotizaciones()
        {
            try
            {
                var usuarioId = GetCurrentUserId();
                var cotizaciones = await _cotizacionService.GetByUsuarioIdAsync(usuarioId);
                return Ok(cotizaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo cotizaciones del usuario {UserId}", GetCurrentUserId());
                return BadRequest(new { message = "Error obteniendo cotizaciones" });
            }
        }
    }
}