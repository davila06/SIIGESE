using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(
            INotificationService notificationService,
            ILogger<NotificationsController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Procesa y envía notificaciones de cobros vencidos
        /// </summary>
        [HttpPost("process-overdue-payments")]
        public async Task<ActionResult<NotificationResultDto>> ProcessOverduePayments()
        {
            try
            {
                var result = await _notificationService.ProcessOverduePaymentsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando cobros vencidos");
                return StatusCode(500, new NotificationResultDto
                {
                    Success = false,
                    Message = $"Error interno del servidor: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Procesa y envía notificaciones de pólizas por vencer
        /// </summary>
        [HttpPost("process-expiring-policies")]
        public async Task<ActionResult<NotificationResultDto>> ProcessExpiringPolicies([FromQuery] int daysBeforeExpiration = 30)
        {
            try
            {
                var result = await _notificationService.ProcessExpiringPoliciesAsync(daysBeforeExpiration);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando pólizas por vencer");
                return StatusCode(500, new NotificationResultDto
                {
                    Success = false,
                    Message = $"Error interno del servidor: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Procesa todas las notificaciones automáticas
        /// </summary>
        [HttpPost("process-all")]
        public async Task<ActionResult<NotificationResultDto>> ProcessAllNotifications([FromQuery] int daysBeforeExpiration = 30)
        {
            try
            {
                var result = await _notificationService.ProcessAllNotificationsAsync(daysBeforeExpiration);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando todas las notificaciones");
                return StatusCode(500, new NotificationResultDto
                {
                    Success = false,
                    Message = $"Error interno del servidor: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Obtiene la lista de cobros vencidos
        /// </summary>
        [HttpGet("overdue-payments")]
        public async Task<ActionResult<List<CobroVencidoDto>>> GetOverduePayments()
        {
            try
            {
                var result = await _notificationService.GetOverduePaymentsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo cobros vencidos");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene la lista de pólizas por vencer
        /// </summary>
        [HttpGet("expiring-policies")]
        public async Task<ActionResult<List<PolizaVencimientoDto>>> GetExpiringPolicies([FromQuery] int daysBeforeExpiration = 30)
        {
            try
            {
                var result = await _notificationService.GetExpiringPoliciesAsync(daysBeforeExpiration);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo pólizas por vencer");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene estadísticas de notificaciones pendientes
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult<NotificationStatisticsDto>> GetStatistics([FromQuery] int daysBeforeExpiration = 30)
        {
            try
            {
                var result = await _notificationService.GetNotificationStatisticsAsync(daysBeforeExpiration);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo estadísticas de notificaciones");
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
