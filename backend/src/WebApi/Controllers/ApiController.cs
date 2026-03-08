using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using Application.DTOs;
using Application.Interfaces;
using System.Security.Claims;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Iniciar sesión de usuario
        /// </summary>
        [HttpPost("login")]
        [EnableRateLimiting("auth")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                var response = await _authService.LoginAsync(request);
                _logger.LogInformation("Usuario {Email} ha iniciado sesión exitosamente", request.Email);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Intento de login fallido para {Email}: {Message}", request.Email, ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el login para {Email}", request.Email);
                return BadRequest(new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Renovar token de autenticación
        /// </summary>
        [HttpPost("refresh-token")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResponseDto))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                var authHeader = Request.Headers["Authorization"].ToString();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    return Unauthorized(new { message = "Token no proporcionado" });
                }

                var token = authHeader.Substring("Bearer ".Length).Trim();
                var response = await _authService.RefreshTokenAsync(token);
                _logger.LogInformation("Token renovado exitosamente para usuario");
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Error al renovar token: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la renovación del token");
                return BadRequest(new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Cerrar sesión de usuario
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                await _authService.LogoutAsync(token);
                
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation("Usuario {UserId} ha cerrado sesión", userId);
                
                return Ok(new { message = "Sesión cerrada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el logout");
                return BadRequest(new { message = "Error cerrando sesión" });
            }
        }

        /// <summary>
        /// Validar token actual
        /// </summary>
        [HttpGet("validate")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ValidateToken()
        {
            try
            {
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var isValid = await _authService.ValidateTokenAsync(token);
                
                if (isValid)
                {
                    var userInfo = new
                    {
                        id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        email = User.FindFirst(ClaimTypes.Email)?.Value,
                        name = User.FindFirst(ClaimTypes.Name)?.Value,
                        roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray()
                    };
                    
                    return Ok(new { valid = true, user = userInfo });
                }
                
                return Unauthorized(new { valid = false });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validando token");
                return Unauthorized(new { valid = false });
            }
        }

        /// <summary>
        /// Cambiar contraseña del usuario actual
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "Usuario no identificado" });

                await _authService.ChangePasswordAsync(int.Parse(userId), request.CurrentPassword, request.NewPassword);
                
                _logger.LogInformation("Usuario {UserId} cambió su contraseña exitosamente", userId);
                return Ok(new { message = "Contraseña cambiada exitosamente" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Intento de cambio de contraseña fallido para usuario {UserId}: {Message}", 
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value, ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cambiando contraseña para usuario {UserId}", 
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return BadRequest(new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Solicitar reset de contraseña
        /// </summary>
        [HttpPost("forgot-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto request)
        {
            try
            {
                await _authService.ForgotPasswordAsync(request.Email);
                
                _logger.LogInformation("Solicitud de reset de contraseña para {Email}", request.Email);
                return Ok(new { message = "Si el email existe, recibirás instrucciones para restablecer tu contraseña" });
            }
            catch (KeyNotFoundException)
            {
                // Por seguridad, devolvemos el mismo mensaje aunque el usuario no exista
                _logger.LogWarning("Solicitud de reset para email no existente: {Email}", request.Email);
                return Ok(new { message = "Si el email existe, recibirás instrucciones para restablecer tu contraseña" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando solicitud de reset para {Email}", request.Email);
                return BadRequest(new { message = "Error procesando solicitud" });
            }
        }

        /// <summary>
        /// Restablecer contraseña usando token
        /// </summary>
        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
        {
            try
            {
                await _authService.ResetPasswordAsync(request.Token, request.NewPassword);
                
                _logger.LogInformation("Contraseña restablecida exitosamente usando token");
                return Ok(new { message = "Contraseña restablecida exitosamente" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Token de reset inválido o expirado: {Message}", ex.Message);
                return BadRequest(new { message = "Token inválido o expirado" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restableciendo contraseña");
                return BadRequest(new { message = "Error interno del servidor" });
            }
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClientesController : ControllerBase
    {
        private readonly IClienteService _clienteService;
        private readonly ILogger<ClientesController> _logger;

        public ClientesController(IClienteService clienteService, ILogger<ClientesController> logger)
        {
            _clienteService = clienteService;
            _logger = logger;
        }

        /// <summary>
        /// Obtener todos los clientes
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ClienteDto>))]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var clientes = await _clienteService.GetAllAsync();
                return Ok(clientes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo clientes");
                return BadRequest(new { message = "Error obteniendo clientes" });
            }
        }

        /// <summary>
        /// Obtener clientes por perfil
        /// </summary>
        [HttpGet("perfil/{perfilId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ClienteDto>))]
        public async Task<IActionResult> GetByPerfil(int perfilId)
        {
            try
            {
                var clientes = await _clienteService.GetByPerfilIdAsync(perfilId);
                return Ok(clientes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo clientes por perfil {PerfilId}", perfilId);
                return BadRequest(new { message = "Error obteniendo clientes" });
            }
        }

        /// <summary>
        /// Obtener cliente por ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClienteDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var cliente = await _clienteService.GetByIdAsync(id);
                if (cliente == null)
                    return NotFound(new { message = "Cliente no encontrado" });

                return Ok(cliente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo cliente {Id}", id);
                return BadRequest(new { message = "Error obteniendo cliente" });
            }
        }

        /// <summary>
        /// Crear nuevo cliente
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,DataLoader")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ClienteDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateClienteDto dto)
        {
            try
            {
                var cliente = await _clienteService.CreateAsync(dto);
                _logger.LogInformation("Cliente creado: {Codigo} por usuario {UserId}", dto.Codigo, GetCurrentUserId());
                return CreatedAtAction(nameof(GetById), new { id = cliente.Id }, cliente);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando cliente");
                return BadRequest(new { message = "Error creando cliente" });
            }
        }

        /// <summary>
        /// Actualizar cliente existente
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClienteDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] CreateClienteDto dto)
        {
            try
            {
                var cliente = await _clienteService.UpdateAsync(id, dto);
                _logger.LogInformation("Cliente {Id} actualizado por usuario {UserId}", id, GetCurrentUserId());
                return Ok(cliente);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Cliente no encontrado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando cliente {Id}", id);
                return BadRequest(new { message = "Error actualizando cliente" });
            }
        }

        /// <summary>
        /// Eliminar cliente
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _clienteService.DeleteAsync(id);
                _logger.LogInformation("Cliente {Id} eliminado por usuario {UserId}", id, GetCurrentUserId());
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Cliente no encontrado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando cliente {Id}", id);
                return BadRequest(new { message = "Error eliminando cliente" });
            }
        }

        /// <summary>
        /// Cargar clientes desde archivo Excel
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

                var userId = GetCurrentUserId();
                var result = await _clienteService.ProcesarExcelAsync(perfilId, file, int.Parse(userId));
                
                _logger.LogInformation("Archivo Excel procesado: {FileName}, {ProcessedRecords} registros procesados por usuario {UserId}", 
                    file.FileName, result.ProcessedRecords, userId);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando archivo Excel");
                return BadRequest(new { message = "Error procesando archivo" });
            }
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0";
        }
    }
}