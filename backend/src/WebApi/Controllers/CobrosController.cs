using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Application.DTOs;
using Application.Interfaces;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CobrosController : ControllerBase
    {
        private readonly ICobrosService _cobrosService;
        private readonly ILogger<CobrosController> _logger;

        public CobrosController(ICobrosService cobrosService, ILogger<CobrosController> logger)
        {
            _cobrosService = cobrosService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los cobros
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CobroDto>>> GetAll()
        {
            try
            {
                var cobros = await _cobrosService.GetAllCobrosAsync();
                return Ok(cobros);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo todos los cobros");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene un cobro por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<CobroDto>> GetById(int id)
        {
            try
            {
                var cobro = await _cobrosService.GetCobroByIdAsync(id);
                if (cobro == null)
                {
                    return NotFound($"Cobro con ID {id} no encontrado");
                }
                return Ok(cobro);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo cobro con ID {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene un cobro por número de recibo
        /// </summary>
        [HttpGet("recibo/{numeroRecibo}")]
        public async Task<ActionResult<CobroDto>> GetByNumeroRecibo(string numeroRecibo)
        {
            try
            {
                var cobro = await _cobrosService.GetCobroByNumeroReciboAsync(numeroRecibo);
                if (cobro == null)
                {
                    return NotFound($"Cobro con número de recibo {numeroRecibo} no encontrado");
                }
                return Ok(cobro);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo cobro con número de recibo {NumeroRecibo}", numeroRecibo);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene cobros por póliza
        /// </summary>
        [HttpGet("poliza/{polizaId}")]
        public async Task<ActionResult<IEnumerable<CobroDto>>> GetByPolizaId(int polizaId)
        {
            try
            {
                var cobros = await _cobrosService.GetCobrosByPolizaIdAsync(polizaId);
                return Ok(cobros);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo cobros de la póliza {PolizaId}", polizaId);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene cobros por estado
        /// </summary>
        [HttpGet("estado/{estado}")]
        public async Task<ActionResult<IEnumerable<CobroDto>>> GetByEstado(string estado)
        {
            try
            {
                var cobros = await _cobrosService.GetCobrosByEstadoAsync(estado);
                return Ok(cobros);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo cobros con estado {Estado}", estado);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene cobros vencidos
        /// </summary>
        [HttpGet("vencidos")]
        public async Task<ActionResult<IEnumerable<CobroDto>>> GetVencidos()
        {
            try
            {
                var cobros = await _cobrosService.GetCobrosVencidosAsync();
                return Ok(cobros);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo cobros vencidos");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene cobros próximos a vencer
        /// </summary>
        [HttpGet("proximos-vencer")]
        public async Task<ActionResult<IEnumerable<CobroDto>>> GetProximosVencer([FromQuery] int dias = 7)
        {
            try
            {
                var cobros = await _cobrosService.GetCobrosProximosVencerAsync(dias);
                return Ok(cobros);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo cobros próximos a vencer en {Dias} días", dias);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene estadísticas de cobros
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<CobroStatsDto>> GetStats()
        {
            try
            {
                var stats = await _cobrosService.GetCobrosStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo estadísticas de cobros");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Crea un nuevo cobro
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<CobroDto>> Create([FromBody] CobroRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var cobro = await _cobrosService.CreateCobroAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = cobro.Id }, cobro);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando cobro");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Actualiza un cobro
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<CobroDto>> Update(int id, [FromBody] ActualizarCobroDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var cobro = await _cobrosService.UpdateCobroAsync(id, request);
                if (cobro == null)
                {
                    return NotFound($"Cobro con ID {id} no encontrado");
                }

                return Ok(cobro);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando cobro con ID {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Registra el pago de un cobro
        /// </summary>
        [HttpPost("registrar-pago")]
        public async Task<ActionResult<CobroDto>> RegistrarPago([FromBody] RegistrarCobroRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var cobro = await _cobrosService.RegistrarCobroAsync(request);
                if (cobro == null)
                {
                    return NotFound($"Cobro con ID {request.CobroId} no encontrado");
                }

                return Ok(cobro);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registrando pago del cobro con ID {CobroId}", request.CobroId);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Elimina un cobro
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var result = await _cobrosService.DeleteCobroAsync(id);
                if (!result)
                {
                    return NotFound($"Cobro con ID {id} no encontrado");
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando cobro con ID {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Genera un nuevo número de recibo
        /// </summary>
        [HttpGet("generar-numero-recibo")]
        public async Task<ActionResult<string>> GenerarNumeroRecibo()
        {
            try
            {
                var numeroRecibo = await _cobrosService.GenerateNumeroReciboAsync();
                return Ok(new { numeroRecibo });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando número de recibo");
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}