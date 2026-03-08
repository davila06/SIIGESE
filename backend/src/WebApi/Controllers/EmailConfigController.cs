using Application.DTOs.EmailConfig;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmailConfigController : ControllerBase
    {
        private readonly IEmailConfigService _emailConfigService;

        public EmailConfigController(IEmailConfigService emailConfigService)
        {
            _emailConfigService = emailConfigService;
        }

        /// <summary>
        /// Obtiene todas las configuraciones de email
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var response = await _emailConfigService.GetAllAsync();
            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Obtiene una configuración de email por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _emailConfigService.GetByIdAsync(id);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Obtiene la configuración de email por defecto
        /// </summary>
        [HttpGet("default")]
        public async Task<IActionResult> GetDefault()
        {
            var response = await _emailConfigService.GetDefaultAsync();
            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Crea una nueva configuración de email
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EmailConfigCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUser = User.FindFirst(ClaimTypes.Email)?.Value ?? "Sistema";
            var response = await _emailConfigService.CreateAsync(dto, currentUser);
            
            return response.Success ? CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response) : BadRequest(response);
        }

        /// <summary>
        /// Actualiza una configuración de email existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] EmailConfigUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUser = User.FindFirst(ClaimTypes.Email)?.Value ?? "Sistema";
            var response = await _emailConfigService.UpdateAsync(id, dto, currentUser);
            
            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Elimina una configuración de email
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _emailConfigService.DeleteAsync(id);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Establece una configuración como predeterminada
        /// </summary>
        [HttpPut("{id}/set-default")]
        public async Task<IActionResult> SetAsDefault(int id)
        {
            var currentUser = User.FindFirst(ClaimTypes.Email)?.Value ?? "Sistema";
            var response = await _emailConfigService.SetAsDefaultAsync(id, currentUser);
            
            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Cambia el estado activo/inactivo de una configuración
        /// </summary>
        [HttpPut("{id}/toggle-status")]
        public async Task<IActionResult> ToggleActiveStatus(int id)
        {
            var currentUser = User.FindFirst(ClaimTypes.Email)?.Value ?? "Sistema";
            var response = await _emailConfigService.ToggleActiveStatusAsync(id, currentUser);
            
            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Prueba una configuración de email enviando un correo de prueba
        /// </summary>
        [HttpPost("test")]
        public async Task<IActionResult> TestConfiguration([FromBody] EmailTestRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _emailConfigService.TestConfigurationAsync(dto);
            return response.Success ? Ok(response) : BadRequest(response);
        }
    }
}