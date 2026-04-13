using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.SignalR;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using System.Security.Claims;
using System.Linq;
using WebApi.Hubs;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CobrosController : ControllerBase
    {
        private readonly ICobrosService _cobrosService;
        private readonly ILogger<CobrosController> _logger;
        private readonly IUserRepository _userRepository;
        private readonly IHubContext<ChatHub> _hubContext;

        public CobrosController(
            ICobrosService cobrosService,
            ILogger<CobrosController> logger,
            IUserRepository userRepository,
            IHubContext<ChatHub> hubContext)
        {
            _cobrosService = cobrosService;
            _logger = logger;
            _userRepository = userRepository;
            _hubContext = hubContext;
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
                if (!Enum.TryParse<EstadoCobro>(estado, true, out var estadoEnum))
                {
                    return BadRequest($"Estado '{estado}' no válido. Estados válidos: {string.Join(", ", Enum.GetNames(typeof(EstadoCobro)))}");
                }
                
                var cobros = await _cobrosService.GetCobrosByEstadoAsync(estadoEnum);
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
        /// Obtiene cobros filtrados por frecuencia de la póliza asociada
        /// (MENSUAL, TRIMESTRAL, SEMESTRAL, ANUAL, BIMESTRAL, CUATRIMESTRAL)
        /// </summary>
        [HttpGet("frecuencia/{frecuencia}")]
        public async Task<ActionResult<IEnumerable<CobroDto>>> GetByFrecuencia(string frecuencia)
        {
            try
            {
                var cobros = await _cobrosService.GetCobrosByFrecuenciaAsync(frecuencia);
                return Ok(cobros);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo cobros con frecuencia {Frecuencia}", frecuencia);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene cobros próximos basados en periodicidad de la póliza:
        /// - Periodicidad MENSUAL: se listan siempre (todos los cobros pendientes)
        /// - Otras periodicidades: se listan con 1 mes de anticipación
        /// </summary>
        [HttpGet("proximos")]
        public async Task<ActionResult<IEnumerable<CobroDto>>> GetProximosPorPeriodicidad()
        {
            try
            {
                var cobros = await _cobrosService.GetCobrosProximosPorPeriodicidadAsync();
                return Ok(cobros);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo cobros próximos por periodicidad");
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
            catch (SqlException ex)
            {
                _logger.LogWarning(ex, "SQL error obteniendo estadísticas de cobros; devolviendo valores por defecto");
                return Ok(new CobroStatsDto());
            }
            catch (TimeoutException ex)
            {
                _logger.LogWarning(ex, "Timeout obteniendo estadísticas de cobros; devolviendo valores por defecto");
                return Ok(new CobroStatsDto());
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
        [Authorize(Roles = "Admin")]
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
        /// Solicita cambio de estado para un cobro (usuarios no admin)
        /// </summary>
        [HttpPost("{id}/estado-change-requests")]
        public async Task<ActionResult<CobroEstadoChangeRequestDto>> SolicitarCambioEstado(int id, [FromBody] SolicitarCambioEstadoCobroDto request)
        {
            try
            {
                if (User.IsInRole("Admin"))
                {
                    return BadRequest("Los usuarios Admin pueden cambiar estado directamente.");
                }

                var userId = GetCurrentUserId();
                var userName = GetCurrentUserName();
                var userEmail = GetCurrentUserEmail();

                var created = await _cobrosService.SolicitarCambioEstadoAsync(
                    id,
                    request.EstadoSolicitado,
                    request.Motivo,
                    userId,
                    userName,
                    userEmail);

                await NotificarAdminsSolicitudPendienteAsync(created, userName);
                return Ok(created);
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
                _logger.LogError(ex, "Error creando solicitud de cambio de estado para cobro {CobroId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Lista solicitudes pendientes de cambio de estado (solo Admin)
        /// </summary>
        [HttpGet("estado-change-requests/pendientes")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<CobroEstadoChangeRequestDto>>> GetSolicitudesPendientes()
        {
            try
            {
                var solicitudes = await _cobrosService.GetSolicitudesPendientesAsync();
                return Ok(solicitudes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo solicitudes pendientes de cambio de estado");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Lista solicitudes de cambio realizadas por el usuario autenticado
        /// </summary>
        [HttpGet("estado-change-requests/mis-solicitudes")]
        public async Task<ActionResult<IEnumerable<CobroEstadoChangeRequestDto>>> GetMisSolicitudes()
        {
            try
            {
                var userId = GetCurrentUserId();
                var solicitudes = await _cobrosService.GetSolicitudesPorSolicitanteAsync(userId);
                return Ok(solicitudes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo solicitudes del usuario autenticado");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Aprueba una solicitud de cambio de estado (solo Admin)
        /// </summary>
        [HttpPost("estado-change-requests/{requestId}/aprobar")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CobroChangeRequestActionResultDto>> AprobarSolicitud(int requestId, [FromBody] ResolverCambioEstadoCobroDto? request = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userName = GetCurrentUserName();
                var result = await _cobrosService.AprobarSolicitudCambioEstadoAsync(requestId, userId, userName, request?.Motivo);

                await NotificarSolicitanteResolucionAsync(result.Request, true);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error aprobando solicitud de cambio de estado {RequestId}", requestId);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Rechaza una solicitud de cambio de estado (solo Admin)
        /// </summary>
        [HttpPost("estado-change-requests/{requestId}/rechazar")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CobroChangeRequestActionResultDto>> RechazarSolicitud(int requestId, [FromBody] ResolverCambioEstadoCobroDto? request = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userName = GetCurrentUserName();
                var result = await _cobrosService.RechazarSolicitudCambioEstadoAsync(requestId, userId, userName, request?.Motivo);

                await NotificarSolicitanteResolucionAsync(result.Request, false);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rechazando solicitud de cambio de estado {RequestId}", requestId);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Registra el pago de un cobro
        /// </summary>
        [HttpPost("registrar-pago")]
        [Authorize(Roles = "Admin")]
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
                await _cobrosService.DeleteCobroAsync(id);
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
        /// Registra el pago de un cobro (endpoint REST por ID)
        /// </summary>
        [HttpPut("{id}/registrar")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CobroDto>> RegistrarPagoById(int id, [FromBody] RegistrarCobroRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                request.CobroId = id;
                var cobro = await _cobrosService.RegistrarCobroAsync(request);
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
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registrando pago del cobro con ID {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Cancela un cobro por ID
        /// </summary>
        [HttpPut("{id}/cancelar")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CobroDto>> CancelarById(int id, [FromBody] CancelarCobroDto? request = null)
        {
            try
            {
                var cobro = await _cobrosService.CancelarCobroAsync(id, request?.Motivo);
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
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelando cobro con ID {Id}", id);
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

        /// <summary>
        /// Genera cobros automáticamente para todas las pólizas activas basándose en su frecuencia de pago
        /// </summary>
        /// <param name="mesesAdelante">Cantidad de meses hacia adelante para generar cobros (por defecto 3)</param>
        [HttpPost("generar-automaticos")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<GenerarCobrosResultDto>> GenerarCobrosAutomaticos([FromQuery] int mesesAdelante = 3)
        {
            try
            {
                _logger.LogInformation("Iniciando generación automática de cobros para {Meses} meses adelante", mesesAdelante);
                var resultado = await _cobrosService.GenerarCobrosAutomaticosAsync(mesesAdelante);
                
                _logger.LogInformation(
                    "Generación completada: {CobrosGenerados} cobros generados, {PolizasProcesadas} pólizas procesadas, {PolizasSaltadas} pólizas saltadas",
                    resultado.CobrosGenerados, resultado.PolizasProcesadas, resultado.PolizasSaltadas);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando cobros automáticos");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Genera cobros automáticamente para una póliza específica basándose en su frecuencia de pago
        /// </summary>
        /// <param name="polizaId">ID de la póliza</param>
        /// <param name="mesesAdelante">Cantidad de meses hacia adelante para generar cobros (por defecto 3)</param>
        [HttpPost("generar-por-poliza/{polizaId}")]
        [Authorize(Roles = "Admin,DataLoader")]
        public async Task<ActionResult<GenerarCobrosResultDto>> GenerarCobrosPorPoliza(int polizaId, [FromQuery] int mesesAdelante = 3)
        {
            try
            {
                _logger.LogInformation("Generando cobros para póliza {PolizaId} con {Meses} meses adelante", polizaId, mesesAdelante);
                var resultado = await _cobrosService.GenerarCobrosPorPolizaAsync(polizaId, mesesAdelante);
                
                if (resultado.Errores.Any())
                {
                    return BadRequest(resultado);
                }

                _logger.LogInformation("Generados {CobrosGenerados} cobros para póliza {PolizaId}", resultado.CobrosGenerados, polizaId);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando cobros para póliza {PolizaId}", polizaId);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Envía un correo electrónico de notificación al destinatario del cobro
        /// </summary>
        [HttpPost("{id}/enviar-email")]
        public async Task<ActionResult> EnviarEmail(int id)
        {
            try
            {
                var (success, message) = await _cobrosService.EnviarEmailCobroAsync(id);
                if (!success)
                    return BadRequest(new { message });
                return Ok(new { message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando email para cobro con ID {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        private int GetCurrentUserId()
        {
            var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(value, out var userId))
            {
                throw new UnauthorizedAccessException("No se encontró el ID del usuario autenticado");
            }

            return userId;
        }

        private string GetCurrentUserName()
        {
            return User.FindFirst(ClaimTypes.Name)?.Value
                ?? User.Identity?.Name
                ?? "Usuario";
        }

        private string GetCurrentUserEmail()
        {
            return User.FindFirst(ClaimTypes.Email)?.Value
                ?? "sin-email@sinseg.local";
        }

        private async Task NotificarAdminsSolicitudPendienteAsync(CobroEstadoChangeRequestDto request, string requestedBy)
        {
            try
            {
                var admins = await _userRepository.GetUsersWithRolesAsync();
                var adminIds = admins
                    .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Admin"))
                    .Select(u => u.Id)
                    .Distinct()
                    .ToList();

                var payload = new
                {
                    type = "cobro-change-request-created",
                    requestId = request.Id,
                    cobroId = request.CobroId,
                    numeroRecibo = request.NumeroRecibo,
                    numeroPoliza = request.NumeroPoliza,
                    estadoActual = request.EstadoActual.ToString(),
                    estadoSolicitado = request.EstadoSolicitado.ToString(),
                    requestedBy,
                    createdAt = request.CreatedAt,
                    message = $"Nueva solicitud de cambio de estado para cobro {request.NumeroRecibo}."
                };

                var tasks = adminIds.Select(adminId =>
                    _hubContext.Clients.Group($"user-{adminId}")
                        .SendAsync("CobroEstadoChangeRequestNotification", payload));

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo enviar push a admins para solicitud {RequestId}", request.Id);
            }
        }

        private async Task NotificarSolicitanteResolucionAsync(CobroEstadoChangeRequestDto request, bool aprobada)
        {
            try
            {
                var payload = new
                {
                    type = aprobada ? "cobro-change-request-approved" : "cobro-change-request-rejected",
                    requestId = request.Id,
                    cobroId = request.CobroId,
                    numeroRecibo = request.NumeroRecibo,
                    estadoActual = request.EstadoActual.ToString(),
                    estadoSolicitado = request.EstadoSolicitado.ToString(),
                    estadoSolicitud = request.EstadoSolicitud.ToString(),
                    resolvedBy = request.ResueltoPorNombre,
                    resolvedAt = request.ResueltoAt,
                    reason = request.MotivoDecision,
                    message = aprobada
                        ? $"Tu solicitud para el cobro {request.NumeroRecibo} fue aprobada."
                        : $"Tu solicitud para el cobro {request.NumeroRecibo} fue rechazada."
                };

                await _hubContext.Clients.Group($"user-{request.SolicitadoPorUserId}")
                    .SendAsync("CobroEstadoChangeRequestNotification", payload);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo enviar push de resolución para solicitud {RequestId}", request.Id);
            }
        }
    }
}