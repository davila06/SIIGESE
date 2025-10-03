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
    [Route("api")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Obtener todos los usuarios
        /// </summary>
        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<UserDto>))]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo usuarios");
                return BadRequest(new { message = "Error obteniendo usuarios" });
            }
        }

        /// <summary>
        /// Obtener usuario por ID
        /// </summary>
        [HttpGet("users/{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUser(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound(new { message = "Usuario no encontrado" });

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo usuario {UserId}", id);
                return BadRequest(new { message = "Error obteniendo usuario" });
            }
        }

        /// <summary>
        /// Crear nuevo usuario
        /// </summary>
        [HttpPost("users")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UserDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            try
            {
                // Verificación adicional de que el usuario actual es Admin
                if (!User.IsInRole("Admin"))
                {
                    _logger.LogWarning("Usuario no administrador {UserId} intentó crear un usuario", GetCurrentUserId());
                    return Forbid("Solo los administradores pueden crear usuarios");
                }

                var user = await _userService.CreateUserAsync(createUserDto);
                _logger.LogInformation("Usuario creado: {UserName} por admin {AdminId}", user.UserName, GetCurrentUserId());
                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando usuario");
                return BadRequest(new { message = "Error creando usuario" });
            }
        }

        /// <summary>
        /// Actualizar usuario
        /// </summary>
        [HttpPut("users/{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
        {
            try
            {
                var user = await _userService.UpdateUserAsync(id, updateUserDto);
                if (user == null)
                    return NotFound(new { message = "Usuario no encontrado" });

                _logger.LogInformation("Usuario actualizado: {UserId} por admin {AdminId}", id, GetCurrentUserId());
                return Ok(user);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando usuario {UserId}", id);
                return BadRequest(new { message = "Error actualizando usuario" });
            }
        }

        /// <summary>
        /// Eliminar usuario
        /// </summary>
        [HttpDelete("users/{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var currentUserId = int.Parse(GetCurrentUserId());
                if (id == currentUserId)
                {
                    return BadRequest(new { message = "No puedes eliminar tu propio usuario" });
                }

                var success = await _userService.DeleteUserAsync(id);
                if (!success)
                    return NotFound(new { message = "Usuario no encontrado" });

                _logger.LogInformation("Usuario eliminado: {UserId} por admin {AdminId}", id, currentUserId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando usuario {UserId}", id);
                return BadRequest(new { message = "Error eliminando usuario" });
            }
        }

        /// <summary>
        /// Obtener todos los roles
        /// </summary>
        [HttpGet("roles")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<RoleDto>))]
        public async Task<IActionResult> GetRoles()
        {
            try
            {
                var roles = await _userService.GetAllRolesAsync();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo roles");
                return BadRequest(new { message = "Error obteniendo roles" });
            }
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0";
        }
    }
}