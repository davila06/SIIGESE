using WebApi.Models.MultiTenant;
using WebApi.Services.MultiTenant;
using System.Security.Claims;

namespace WebApi.Middleware
{
    /// <summary>
    /// Middleware para detectar y configurar el tenant actual en cada request
    /// </summary>
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantMiddleware> _logger;

        public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
        {
            var tenantId = await ResolveTenantIdAsync(context, tenantService);
            
            if (!string.IsNullOrEmpty(tenantId))
            {
                var tenantInfo = await tenantService.GetTenantByIdAsync(tenantId);
                
                if (tenantInfo != null && tenantInfo.IsActive)
                {
                    // Establecer información del tenant en el contexto
                    context.Items["TenantInfo"] = tenantInfo;
                    context.Items["TenantId"] = tenantId;
                    
                    // Agregar claims al usuario si está autenticado
                    if (context.User.Identity?.IsAuthenticated == true)
                    {
                        var identity = context.User.Identity as ClaimsIdentity;
                        if (identity != null && !identity.HasClaim("tenant_id", tenantId))
                        {
                            identity.AddClaim(new Claim("tenant_id", tenantId));
                            identity.AddClaim(new Claim("company_name", tenantInfo.CompanyName));
                            identity.AddClaim(new Claim("subscription_plan", tenantInfo.SubscriptionPlan));
                        }
                    }
                    
                    _logger.LogDebug("Tenant {TenantId} ({CompanyName}) resolved for request {Path}", 
                        tenantId, tenantInfo.CompanyName, context.Request.Path);
                }
                else if (tenantInfo != null && !tenantInfo.IsActive)
                {
                    _logger.LogWarning("Inactive tenant {TenantId} attempted to access {Path}", tenantId, context.Request.Path);
                    
                    // Tenant inactivo - retornar error 403
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsync("Tenant is not active");
                    return;
                }
                else
                {
                    _logger.LogWarning("Invalid tenant {TenantId} attempted to access {Path}", tenantId, context.Request.Path);
                }
            }
            else
            {
                // Para rutas que no requieren tenant (login, health check, etc.)
                var path = context.Request.Path.Value?.ToLower() ?? "";
                var isPublicRoute = IsPublicRoute(path);
                
                if (!isPublicRoute)
                {
                    _logger.LogWarning("No tenant resolved for protected route {Path}", context.Request.Path);
                }
            }

            await _next(context);
        }

        private async Task<string?> ResolveTenantIdAsync(HttpContext context, ITenantService tenantService)
        {
            string? tenantId = null;

            // 1. Desde subdomain (empresa-abc.siinadseg.com)
            tenantId = await ResolveTenantFromSubdomainAsync(context, tenantService);
            if (!string.IsNullOrEmpty(tenantId)) return tenantId;

            // 2. Desde header X-Tenant-ID (para APIs)
            tenantId = ResolveTenantFromHeader(context);
            if (!string.IsNullOrEmpty(tenantId)) return tenantId;

            // 3. Desde query parameter (para desarrollo/testing)
            tenantId = ResolveTenantFromQuery(context);
            if (!string.IsNullOrEmpty(tenantId)) return tenantId;

            // 4. Desde JWT claims (usuario ya autenticado)
            tenantId = ResolveTenantFromJwtClaims(context);
            if (!string.IsNullOrEmpty(tenantId)) return tenantId;

            // 5. Desde cookies (último recurso)
            tenantId = ResolveTenantFromCookie(context);
            if (!string.IsNullOrEmpty(tenantId)) return tenantId;

            return null;
        }

        private async Task<string?> ResolveTenantFromSubdomainAsync(HttpContext context, ITenantService tenantService)
        {
            try
            {
                var host = context.Request.Host.Value;
                
                if (string.IsNullOrEmpty(host))
                    return null;

                // Detectar subdomain pattern: tenant.siinadseg.com
                if (host.Contains(".siinadseg.com"))
                {
                    var parts = host.Split('.');
                    if (parts.Length >= 3 && parts[0] != "www" && parts[0] != "api")
                    {
                        var potentialTenantId = parts[0];
                        
                        // Validar que el tenant existe
                        if (await tenantService.TenantExistsAsync(potentialTenantId))
                        {
                            return potentialTenantId;
                        }
                    }
                }

                // Detectar custom domain (ejemplo.com -> buscar por domain)
                if (!host.Contains(".siinadseg.com") && !host.Contains("localhost") && !host.Contains("127.0.0.1"))
                {
                    var tenantInfo = await tenantService.GetTenantByDomainAsync(host);
                    return tenantInfo?.TenantId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving tenant from subdomain: {Host}", context.Request.Host);
            }

            return null;
        }

        private string? ResolveTenantFromHeader(HttpContext context)
        {
            try
            {
                var tenantHeader = context.Request.Headers["X-Tenant-ID"].FirstOrDefault();
                
                if (!string.IsNullOrWhiteSpace(tenantHeader))
                {
                    return tenantHeader.Trim();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving tenant from header");
            }

            return null;
        }

        private string? ResolveTenantFromQuery(HttpContext context)
        {
            try
            {
                var tenantQuery = context.Request.Query["tenant"].FirstOrDefault();
                
                if (!string.IsNullOrWhiteSpace(tenantQuery))
                {
                    return tenantQuery.Trim();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving tenant from query parameter");
            }

            return null;
        }

        private string? ResolveTenantFromJwtClaims(HttpContext context)
        {
            try
            {
                if (context.User?.Identity?.IsAuthenticated == true)
                {
                    var tenantClaim = context.User.FindFirst("tenant_id")?.Value;
                    
                    if (!string.IsNullOrWhiteSpace(tenantClaim))
                    {
                        return tenantClaim.Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving tenant from JWT claims");
            }

            return null;
        }

        private string? ResolveTenantFromCookie(HttpContext context)
        {
            try
            {
                var tenantCookie = context.Request.Cookies["X-Tenant-ID"];
                
                if (!string.IsNullOrWhiteSpace(tenantCookie))
                {
                    return tenantCookie.Trim();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving tenant from cookie");
            }

            return null;
        }

        private bool IsPublicRoute(string path)
        {
            var publicRoutes = new[]
            {
                "/health",
                "/healthcheck",
                "/api/health",
                "/api/auth/login",
                "/api/auth/register",
                "/api/auth/forgot-password",
                "/api/auth/reset-password",
                "/api/tenants/create", // Para self-service tenant creation
                "/swagger",
                "/api/system", // Rutas de sistema
                "/.well-known"
            };

            return publicRoutes.Any(route => path.StartsWith(route));
        }
    }

    /// <summary>
    /// Helper class para acceder al tenant actual desde controllers/services
    /// </summary>
    public static class TenantContextExtensions
    {
        public static TenantInfo? GetTenantInfo(this HttpContext context)
        {
            return context.Items["TenantInfo"] as TenantInfo;
        }

        public static string? GetTenantId(this HttpContext context)
        {
            return context.Items["TenantId"] as string;
        }

        public static bool HasTenant(this HttpContext context)
        {
            return context.GetTenantId() != null;
        }

        public static void SetTenantInfo(this HttpContext context, TenantInfo tenantInfo)
        {
            context.Items["TenantInfo"] = tenantInfo;
            context.Items["TenantId"] = tenantInfo.TenantId;
        }
    }

    /// <summary>
    /// Service para inyectar información del tenant en controllers
    /// </summary>
    public interface ITenantContext
    {
        TenantInfo? CurrentTenant { get; }
        string? TenantId { get; }
        bool HasTenant { get; }
    }

    public class TenantContext : ITenantContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public TenantInfo? CurrentTenant => _httpContextAccessor.HttpContext?.GetTenantInfo();
        public string? TenantId => _httpContextAccessor.HttpContext?.GetTenantId();
        public bool HasTenant => !string.IsNullOrEmpty(TenantId);
    }

    /// <summary>
    /// Extension methods para registrar el middleware
    /// </summary>
    public static class TenantMiddlewareExtensions
    {
        public static IApplicationBuilder UseTenantMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TenantMiddleware>();
        }

        public static IServiceCollection AddTenantServices(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<ITenantContext, TenantContext>();
            services.AddScoped<ITenantService, TenantService>();
            
            return services;
        }
    }
}