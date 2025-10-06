using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebApi.Services.MultiTenant;
using WebApi.Models.MultiTenant;
using WebApi.Middleware;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TenantsController : ControllerBase
    {
        private readonly ITenantService _tenantService;
        private readonly ITenantContext _tenantContext;
        private readonly ILogger<TenantsController> _logger;

        public TenantsController(
            ITenantService tenantService,
            ITenantContext tenantContext,
            ILogger<TenantsController> logger)
        {
            _tenantService = tenantService;
            _tenantContext = tenantContext;
            _logger = logger;
        }

        /// <summary>
        /// Obtener información del tenant actual
        /// </summary>
        [HttpGet("current")]
        [Authorize]
        public async Task<ActionResult<TenantResponse>> GetCurrentTenant()
        {
            if (!_tenantContext.HasTenant)
            {
                return BadRequest("No tenant context available");
            }

            var tenantInfo = _tenantContext.CurrentTenant;
            if (tenantInfo == null)
            {
                return NotFound("Tenant not found");
            }

            var response = new TenantResponse
            {
                TenantId = tenantInfo.TenantId,
                CompanyName = tenantInfo.CompanyName,
                Domain = tenantInfo.Domain,
                LogoUrl = tenantInfo.LogoUrl,
                PrimaryColor = tenantInfo.PrimaryColor,
                SecondaryColor = tenantInfo.SecondaryColor,
                IsActive = tenantInfo.IsActive,
                SubscriptionPlan = tenantInfo.SubscriptionPlan,
                MaxUsers = tenantInfo.MaxUsers,
                MaxPolizas = tenantInfo.MaxPolizas,
                CreatedAt = tenantInfo.CreatedAt
            };

            return Ok(response);
        }

        /// <summary>
        /// Obtener tenant por ID (solo super admin)
        /// </summary>
        [HttpGet("{tenantId}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<TenantResponse>> GetTenant(string tenantId)
        {
            var tenantInfo = await _tenantService.GetTenantByIdAsync(tenantId);
            
            if (tenantInfo == null)
            {
                return NotFound($"Tenant '{tenantId}' not found");
            }

            var response = new TenantResponse
            {
                TenantId = tenantInfo.TenantId,
                CompanyName = tenantInfo.CompanyName,
                Domain = tenantInfo.Domain,
                LogoUrl = tenantInfo.LogoUrl,
                PrimaryColor = tenantInfo.PrimaryColor,
                SecondaryColor = tenantInfo.SecondaryColor,
                IsActive = tenantInfo.IsActive,
                SubscriptionPlan = tenantInfo.SubscriptionPlan,
                MaxUsers = tenantInfo.MaxUsers,
                MaxPolizas = tenantInfo.MaxPolizas,
                CreatedAt = tenantInfo.CreatedAt
            };

            return Ok(response);
        }

        /// <summary>
        /// Listar todos los tenants (solo super admin)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<List<TenantResponse>>> GetAllTenants()
        {
            var tenants = await _tenantService.GetAllTenantsAsync();
            return Ok(tenants);
        }

        /// <summary>
        /// Crear nuevo tenant
        /// </summary>
        [HttpPost]
        [AllowAnonymous] // Para permitir self-service tenant creation
        public async Task<ActionResult<TenantResponse>> CreateTenant([FromBody] CreateTenantRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validar formato del TenantId
            if (!IsValidTenantId(request.TenantId))
            {
                return BadRequest("TenantId must contain only lowercase letters, numbers, and hyphens");
            }

            var result = await _tenantService.CreateTenantAsync(request);
            
            if (result == null)
            {
                return Conflict("Tenant with this ID or domain already exists");
            }

            _logger.LogInformation("New tenant created: {TenantId} by {Email}", request.TenantId, request.AdminEmail);

            return CreatedAtAction(nameof(GetTenant), new { tenantId = result.TenantId }, result);
        }

        /// <summary>
        /// Actualizar tenant actual
        /// </summary>
        [HttpPut("current")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> UpdateCurrentTenant([FromBody] UpdateTenantRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!_tenantContext.HasTenant)
            {
                return BadRequest("No tenant context available");
            }

            var tenantId = _tenantContext.TenantId!;
            var success = await _tenantService.UpdateTenantAsync(tenantId, request);
            
            if (!success)
            {
                return BadRequest("Failed to update tenant or domain already exists");
            }

            _logger.LogInformation("Tenant {TenantId} updated", tenantId);

            return NoContent();
        }

        /// <summary>
        /// Actualizar tenant específico (solo super admin)
        /// </summary>
        [HttpPut("{tenantId}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> UpdateTenant(string tenantId, [FromBody] UpdateTenantRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _tenantService.UpdateTenantAsync(tenantId, request);
            
            if (!success)
            {
                return BadRequest("Failed to update tenant, tenant not found, or domain already exists");
            }

            _logger.LogInformation("Tenant {TenantId} updated by super admin", tenantId);

            return NoContent();
        }

        /// <summary>
        /// Activar tenant (solo super admin)
        /// </summary>
        [HttpPost("{tenantId}/activate")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> ActivateTenant(string tenantId)
        {
            var success = await _tenantService.ActivateTenantAsync(tenantId);
            
            if (!success)
            {
                return NotFound("Tenant not found");
            }

            _logger.LogInformation("Tenant {TenantId} activated", tenantId);

            return NoContent();
        }

        /// <summary>
        /// Desactivar tenant (solo super admin)
        /// </summary>
        [HttpPost("{tenantId}/deactivate")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DeactivateTenant(string tenantId)
        {
            var success = await _tenantService.DeactivateTenantAsync(tenantId);
            
            if (!success)
            {
                return NotFound("Tenant not found");
            }

            _logger.LogInformation("Tenant {TenantId} deactivated", tenantId);

            return NoContent();
        }

        /// <summary>
        /// Obtener métricas de uso del tenant actual
        /// </summary>
        [HttpGet("current/usage")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult<TenantUsageResponse>> GetCurrentTenantUsage()
        {
            if (!_tenantContext.HasTenant)
            {
                return BadRequest("No tenant context available");
            }

            var usage = await _tenantService.GetTenantUsageAsync(_tenantContext.TenantId!);
            
            if (usage == null)
            {
                return NotFound("Usage data not found");
            }

            return Ok(usage);
        }

        /// <summary>
        /// Obtener métricas de uso de tenant específico (solo super admin)
        /// </summary>
        [HttpGet("{tenantId}/usage")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<TenantUsageResponse>> GetTenantUsage(string tenantId)
        {
            var usage = await _tenantService.GetTenantUsageAsync(tenantId);
            
            if (usage == null)
            {
                return NotFound("Tenant or usage data not found");
            }

            return Ok(usage);
        }

        /// <summary>
        /// Verificar disponibilidad de tenant ID
        /// </summary>
        [HttpGet("check-availability/{tenantId}")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> CheckTenantAvailability(string tenantId)
        {
            if (!IsValidTenantId(tenantId))
            {
                return BadRequest(new { available = false, reason = "Invalid format" });
            }

            var exists = await _tenantService.TenantExistsAsync(tenantId);
            
            return Ok(new { available = !exists, tenantId });
        }

        /// <summary>
        /// Obtener configuración de branding del tenant actual
        /// </summary>
        [HttpGet("current/branding")]
        [AllowAnonymous] // Permite acceso público para mostrar branding
        public async Task<ActionResult<object>> GetCurrentTenantBranding()
        {
            var tenantInfo = _tenantContext.CurrentTenant;
            
            if (tenantInfo == null)
            {
                // Retornar branding por defecto
                return Ok(new
                {
                    companyName = "SIINADSEG",
                    logoUrl = "/assets/default-logo.png",
                    primaryColor = "#1976d2",
                    secondaryColor = "#424242",
                    customCss = ""
                });
            }

            return Ok(new
            {
                companyName = tenantInfo.CompanyName,
                logoUrl = tenantInfo.LogoUrl ?? "/assets/default-logo.png",
                primaryColor = tenantInfo.PrimaryColor,
                secondaryColor = tenantInfo.SecondaryColor,
                customCss = tenantInfo.CustomCss ?? ""
            });
        }

        /// <summary>
        /// Limpiar cache del tenant actual
        /// </summary>
        [HttpPost("current/clear-cache")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public IActionResult ClearCurrentTenantCache()
        {
            if (!_tenantContext.HasTenant)
            {
                return BadRequest("No tenant context available");
            }

            _tenantService.ClearTenantCache(_tenantContext.TenantId!);
            
            _logger.LogInformation("Cache cleared for tenant {TenantId}", _tenantContext.TenantId);

            return Ok(new { message = "Cache cleared successfully" });
        }

        private bool IsValidTenantId(string tenantId)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
                return false;

            // Solo letras minúsculas, números y guiones
            // Longitud entre 3 y 50 caracteres
            // No puede empezar o terminar con guión
            return tenantId.Length >= 3 && 
                   tenantId.Length <= 50 && 
                   tenantId.All(c => char.IsLower(c) || char.IsDigit(c) || c == '-') &&
                   !tenantId.StartsWith('-') && 
                   !tenantId.EndsWith('-') &&
                   !tenantId.Contains("--"); // No guiones consecutivos
        }
    }
}