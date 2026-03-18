using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableRateLimiting("api")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IChatService chatService, ILogger<ChatController> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException("User ID claim not found"));

        // ── Sessions ─────────────────────────────────────────────────────────────

        /// <summary>Get all chat sessions for the authenticated user</summary>
        [HttpGet("sessions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSessions()
        {
            try
            {
                var sessions = await _chatService.GetSessionsAsync(GetUserId());
                return Ok(sessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching chat sessions");
                return BadRequest(new { message = "Error al obtener las sesiones de chat" });
            }
        }

        /// <summary>Create a new chat session</summary>
        [HttpPost("sessions")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateSession([FromBody] CreateChatSessionDto dto)
        {
            try
            {
                var session = await _chatService.CreateSessionAsync(GetUserId(), dto.Title);
                return CreatedAtAction(nameof(GetSession), new { sessionId = session.SessionId }, session);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating chat session");
                return BadRequest(new { message = "Error al crear la sesión de chat" });
            }
        }

        /// <summary>Get a session with its messages</summary>
        [HttpGet("sessions/{sessionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSession(string sessionId)
        {
            try
            {
                var detail = await _chatService.GetSessionAsync(GetUserId(), sessionId);
                return Ok(detail);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Sesión {sessionId} no encontrada" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching chat session {SessionId}", sessionId);
                return BadRequest(new { message = "Error al obtener la sesión" });
            }
        }

        /// <summary>Delete (soft-delete) a chat session</summary>
        [HttpDelete("sessions/{sessionId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteSession(string sessionId)
        {
            try
            {
                await _chatService.DeleteSessionAsync(GetUserId(), sessionId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Sesión {sessionId} no encontrada" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting chat session {SessionId}", sessionId);
                return BadRequest(new { message = "Error al eliminar la sesión" });
            }
        }

        /// <summary>Mark all messages in a session as read</summary>
        [HttpPatch("sessions/{sessionId}/read")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> MarkAsRead(string sessionId)
        {
            try
            {
                await _chatService.MarkSessionAsReadAsync(GetUserId(), sessionId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Sesión {sessionId} no encontrada" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking session as read");
                return BadRequest(new { message = "Error al marcar como leído" });
            }
        }

        // ── Messages ──────────────────────────────────────────────────────────────

        /// <summary>Send a message and get an AI response</summary>
        [HttpPost("sessions/{sessionId}/messages")]
        [EnableRateLimiting("auth")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SendMessage(string sessionId, [FromBody] SendMessageDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Content) || dto.Content.Trim().Length < 1)
                return BadRequest(new { message = "El mensaje no puede estar vacío" });

            if (dto.Content.Length > 1000)
                return BadRequest(new { message = "El mensaje excede el límite de 1000 caracteres" });

            try
            {
                var response = await _chatService.SendMessageAsync(GetUserId(), sessionId, dto);
                return Ok(response);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Sesión {sessionId} no encontrada" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat message for session {SessionId}", sessionId);
                return BadRequest(new { message = "Error al procesar el mensaje" });
            }
        }

        // ── Reactions ─────────────────────────────────────────────────────────────

        /// <summary>React to a bot message (like/dislike)</summary>
        [HttpPatch("messages/{messageId}/reaction")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> ReactToMessage(int messageId, [FromBody] ReactToMessageDto dto)
        {
            if (dto.Score is not (1 or -1))
                return BadRequest(new { message = "Score debe ser 1 (like) o -1 (dislike)" });

            try
            {
                await _chatService.ReactToMessageAsync(GetUserId(), messageId, dto);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Mensaje {messageId} no encontrado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reacting to message {MessageId}", messageId);
                return BadRequest(new { message = "Error al registrar la reacción" });
            }
        }

        // ── Stats ─────────────────────────────────────────────────────────────────

        /// <summary>Get chat usage statistics for the authenticated user</summary>
        [HttpGet("stats")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var stats = await _chatService.GetStatsAsync(GetUserId());
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching chat stats");
                return BadRequest(new { message = "Error al obtener estadísticas" });
            }
        }
    }
}
