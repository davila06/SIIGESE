using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmailController : ControllerBase
    {
        private readonly IEmailDashboardService _emailDashboardService;

        public EmailController(IEmailDashboardService emailDashboardService)
        {
            _emailDashboardService = emailDashboardService;
        }

        /// <summary>
        /// Obtiene estadísticas generales de emails
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var response = await _emailDashboardService.GetStatsAsync();
            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Envía un email individual
        /// </summary>
        [HttpPost("send")]
        public async Task<IActionResult> SendEmail([FromBody] EmailRequestDto request)
        {
            var response = await _emailDashboardService.SendEmailAsync(request);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Envía emails masivos
        /// </summary>
        [HttpPost("send-bulk")]
        public async Task<IActionResult> SendBulkEmails([FromBody] BulkEmailRequestDto request)
        {
            var response = await _emailDashboardService.SendBulkEmailsAsync(request);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Envía notificación de cobro vencido
        /// </summary>
        [HttpPost("send-cobro-vencido")]
        public async Task<IActionResult> SendCobroVencido([FromBody] CobroVencidoEmailRequestDto request)
        {
            var response = await _emailDashboardService.SendCobroVencidoEmailAsync(request);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Envía notificación de reclamo recibido al cliente
        /// </summary>
        [HttpPost("send-reclamo-recibido")]
        public async Task<IActionResult> SendReclamoRecibido([FromBody] ReclamoRecibidoEmailRequestDto request)
        {
            var response = await _emailDashboardService.SendReclamoRecibidoEmailAsync(request);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Envía email de bienvenida a un nuevo usuario
        /// </summary>
        [HttpPost("send-bienvenida")]
        public async Task<IActionResult> SendBienvenida([FromBody] BienvenidaEmailRequestDto request)
        {
            var response = await _emailDashboardService.SendBienvenidaEmailAsync(request);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Obtiene el historial de emails enviados
        /// </summary>
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var response = await _emailDashboardService.GetEmailHistoryAsync(pageNumber, pageSize);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Envía automáticamente notificaciones a clientes con cobros vencidos
        /// </summary>
        [HttpPost("automatic/cobros-vencidos")]
        public async Task<IActionResult> SendAutomaticCobroVencidos()
        {
            var response = await _emailDashboardService.SendAutomaticCobroVencidoNotificationsAsync();
            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Envía automáticamente notificaciones a clientes con pólizas próximas a vencer
        /// </summary>
        [HttpPost("automatic/polizas-por-vencer")]
        public async Task<IActionResult> SendAutomaticPolizasPorVencer()
        {
            var response = await _emailDashboardService.SendAutomaticPolizasPorVencerNotificationsAsync();
            return response.Success ? Ok(response) : BadRequest(response);
        }
    }
}
