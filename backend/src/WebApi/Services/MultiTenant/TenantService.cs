using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using WebApi.Data.MultiTenant;
using WebApi.Models.MultiTenant;

namespace WebApi.Services.MultiTenant
{
    public interface ITenantService
    {
        Task<TenantInfo?> GetTenantByIdAsync(string tenantId);
        Task<TenantInfo?> GetTenantByDomainAsync(string domain);
        Task<string?> GetConnectionStringAsync(string tenantId);
        Task<bool> TenantExistsAsync(string tenantId);
        Task<bool> IsTenantActiveAsync(string tenantId);
        Task<List<TenantResponse>> GetAllTenantsAsync();
        Task<TenantResponse?> CreateTenantAsync(CreateTenantRequest request);
        Task<bool> UpdateTenantAsync(string tenantId, UpdateTenantRequest request);
        Task<bool> ActivateTenantAsync(string tenantId);
        Task<bool> DeactivateTenantAsync(string tenantId);
        Task<TenantUsageResponse?> GetTenantUsageAsync(string tenantId);
        void ClearTenantCache(string tenantId);
    }

    public class TenantService : ITenantService
    {
        private readonly MasterDbContext _masterContext;
        private readonly IMemoryCache _cache;
        private readonly ILogger<TenantService> _logger;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(1);

        public TenantService(
            MasterDbContext masterContext,
            IMemoryCache cache,
            ILogger<TenantService> logger)
        {
            _masterContext = masterContext;
            _cache = cache;
            _logger = logger;
        }

        public async Task<TenantInfo?> GetTenantByIdAsync(string tenantId)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
                return null;

            var cacheKey = $"tenant_id_{tenantId}";
            
            if (_cache.TryGetValue(cacheKey, out TenantInfo? cachedTenant))
            {
                _logger.LogDebug("Tenant {TenantId} retrieved from cache", tenantId);
                return cachedTenant;
            }

            try
            {
                var tenant = await _masterContext.Tenants
                    .Where(t => t.TenantId == tenantId && t.IsActive)
                    .Select(t => new TenantInfo
                    {
                        TenantId = t.TenantId,
                        CompanyName = t.CompanyName,
                        DatabaseName = t.DatabaseName,
                        ConnectionString = t.ConnectionString,
                        LogoUrl = t.LogoUrl,
                        PrimaryColor = t.PrimaryColor,
                        SecondaryColor = t.SecondaryColor,
                        CustomCss = t.CustomCss,
                        IsActive = t.IsActive,
                        SubscriptionPlan = t.SubscriptionPlan,
                        MaxUsers = t.MaxUsers,
                        MaxPolizas = t.MaxPolizas,
                        Domain = t.Domain,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                if (tenant != null)
                {
                    _cache.Set(cacheKey, tenant, _cacheExpiration);
                    _logger.LogInformation("Tenant {TenantId} retrieved from database and cached", tenantId);
                }
                else
                {
                    _logger.LogWarning("Tenant {TenantId} not found or inactive", tenantId);
                }

                return tenant;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tenant {TenantId}", tenantId);
                return null;
            }
        }

        public async Task<TenantInfo?> GetTenantByDomainAsync(string domain)
        {
            if (string.IsNullOrWhiteSpace(domain))
                return null;

            var cacheKey = $"tenant_domain_{domain}";
            
            if (_cache.TryGetValue(cacheKey, out TenantInfo? cachedTenant))
            {
                _logger.LogDebug("Tenant with domain {Domain} retrieved from cache", domain);
                return cachedTenant;
            }

            try
            {
                var tenant = await _masterContext.Tenants
                    .Where(t => t.Domain == domain && t.IsActive)
                    .Select(t => new TenantInfo
                    {
                        TenantId = t.TenantId,
                        CompanyName = t.CompanyName,
                        DatabaseName = t.DatabaseName,
                        ConnectionString = t.ConnectionString,
                        LogoUrl = t.LogoUrl,
                        PrimaryColor = t.PrimaryColor,
                        SecondaryColor = t.SecondaryColor,
                        CustomCss = t.CustomCss,
                        IsActive = t.IsActive,
                        SubscriptionPlan = t.SubscriptionPlan,
                        MaxUsers = t.MaxUsers,
                        MaxPolizas = t.MaxPolizas,
                        Domain = t.Domain,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                if (tenant != null)
                {
                    _cache.Set(cacheKey, tenant, _cacheExpiration);
                    _cache.Set($"tenant_id_{tenant.TenantId}", tenant, _cacheExpiration);
                    _logger.LogInformation("Tenant with domain {Domain} retrieved from database and cached", domain);
                }
                else
                {
                    _logger.LogWarning("Tenant with domain {Domain} not found or inactive", domain);
                }

                return tenant;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tenant by domain {Domain}", domain);
                return null;
            }
        }

        public async Task<string?> GetConnectionStringAsync(string tenantId)
        {
            var tenant = await GetTenantByIdAsync(tenantId);
            return tenant?.ConnectionString;
        }

        public async Task<bool> TenantExistsAsync(string tenantId)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
                return false;

            try
            {
                return await _masterContext.Tenants
                    .AnyAsync(t => t.TenantId == tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if tenant {TenantId} exists", tenantId);
                return false;
            }
        }

        public async Task<bool> IsTenantActiveAsync(string tenantId)
        {
            var tenant = await GetTenantByIdAsync(tenantId);
            return tenant?.IsActive ?? false;
        }

        public async Task<List<TenantResponse>> GetAllTenantsAsync()
        {
            try
            {
                return await _masterContext.Tenants
                    .OrderBy(t => t.CompanyName)
                    .Select(t => new TenantResponse
                    {
                        TenantId = t.TenantId,
                        CompanyName = t.CompanyName,
                        Domain = t.Domain,
                        LogoUrl = t.LogoUrl,
                        PrimaryColor = t.PrimaryColor,
                        SecondaryColor = t.SecondaryColor,
                        IsActive = t.IsActive,
                        SubscriptionPlan = t.SubscriptionPlan,
                        MaxUsers = t.MaxUsers,
                        MaxPolizas = t.MaxPolizas,
                        CreatedAt = t.CreatedAt,
                        ContactName = t.ContactName,
                        ContactEmail = t.ContactEmail
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all tenants");
                return new List<TenantResponse>();
            }
        }

        public async Task<TenantResponse?> CreateTenantAsync(CreateTenantRequest request)
        {
            try
            {
                // Validar que no exista el tenant
                if (await TenantExistsAsync(request.TenantId))
                {
                    _logger.LogWarning("Attempted to create tenant with existing ID: {TenantId}", request.TenantId);
                    return null;
                }

                // Validar dominio único si se proporciona
                if (!string.IsNullOrWhiteSpace(request.Domain))
                {
                    var domainExists = await _masterContext.Tenants
                        .AnyAsync(t => t.Domain == request.Domain);
                    
                    if (domainExists)
                    {
                        _logger.LogWarning("Attempted to create tenant with existing domain: {Domain}", request.Domain);
                        return null;
                    }
                }

                using var transaction = await _masterContext.Database.BeginTransactionAsync();

                try
                {
                    var databaseName = $"SinsegTenant_{request.TenantId}";
                    var connectionString = $"Server=(localdb)\\mssqllocaldb;Database={databaseName};Trusted_Connection=true;MultipleActiveResultSets=true";

                    // Crear tenant
                    var tenant = new Tenant
                    {
                        TenantId = request.TenantId,
                        CompanyName = request.CompanyName,
                        Domain = request.Domain,
                        DatabaseName = databaseName,
                        ConnectionString = connectionString,
                        SubscriptionPlan = request.SubscriptionPlan,
                        LogoUrl = request.LogoUrl,
                        PrimaryColor = request.PrimaryColor,
                        SecondaryColor = request.SecondaryColor,
                        ContactEmail = request.AdminEmail,
                        BillingEmail = request.AdminEmail
                    };

                    _masterContext.Tenants.Add(tenant);

                    // Crear usuario admin
                    var adminUser = new SystemUser
                    {
                        Email = request.AdminEmail,
                        PasswordHash = "$2a$11$wK0FZHR6o.l1Q8q8dXmQ7uWJ9u5HJV8uJEXFhVz7Q8YWOzQU6J6nK", // Temporal: Admin@123
                        FirstName = request.AdminFirstName,
                        LastName = request.AdminLastName
                    };

                    _masterContext.SystemUsers.Add(adminUser);
                    await _masterContext.SaveChangesAsync();

                    // Relacionar usuario con tenant
                    var userTenant = new UserTenant
                    {
                        UserId = adminUser.Id,
                        TenantId = request.TenantId,
                        Role = "Admin"
                    };

                    _masterContext.UserTenants.Add(userTenant);

                    // Agregar configuraciones básicas
                    var configurations = new[]
                    {
                        new TenantConfiguration { TenantId = request.TenantId, ConfigKey = "TIMEZONE", ConfigValue = "America/Argentina/Buenos_Aires", ConfigType = "String" },
                        new TenantConfiguration { TenantId = request.TenantId, ConfigKey = "CURRENCY", ConfigValue = "ARS", ConfigType = "String" },
                        new TenantConfiguration { TenantId = request.TenantId, ConfigKey = "LANGUAGE", ConfigValue = "es-AR", ConfigType = "String" },
                        new TenantConfiguration { TenantId = request.TenantId, ConfigKey = "ENABLE_NOTIFICATIONS", ConfigValue = "true", ConfigType = "Boolean" }
                    };

                    _masterContext.TenantConfigurations.AddRange(configurations);
                    await _masterContext.SaveChangesAsync();

                    await transaction.CommitAsync();

                    _logger.LogInformation("Tenant {TenantId} created successfully", request.TenantId);

                    return new TenantResponse
                    {
                        TenantId = tenant.TenantId,
                        CompanyName = tenant.CompanyName,
                        Domain = tenant.Domain,
                        LogoUrl = tenant.LogoUrl,
                        PrimaryColor = tenant.PrimaryColor,
                        SecondaryColor = tenant.SecondaryColor,
                        IsActive = tenant.IsActive,
                        SubscriptionPlan = tenant.SubscriptionPlan,
                        MaxUsers = tenant.MaxUsers,
                        MaxPolizas = tenant.MaxPolizas,
                        CreatedAt = tenant.CreatedAt,
                        ContactEmail = tenant.ContactEmail
                    };
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tenant {TenantId}", request.TenantId);
                return null;
            }
        }

        public async Task<bool> UpdateTenantAsync(string tenantId, UpdateTenantRequest request)
        {
            try
            {
                var tenant = await _masterContext.Tenants
                    .FirstOrDefaultAsync(t => t.TenantId == tenantId);

                if (tenant == null)
                {
                    _logger.LogWarning("Attempted to update non-existent tenant: {TenantId}", tenantId);
                    return false;
                }

                // Validar dominio único
                if (!string.IsNullOrWhiteSpace(request.Domain) && request.Domain != tenant.Domain)
                {
                    var domainExists = await _masterContext.Tenants
                        .AnyAsync(t => t.Domain == request.Domain && t.TenantId != tenantId);
                    
                    if (domainExists)
                    {
                        _logger.LogWarning("Attempted to update tenant {TenantId} with existing domain: {Domain}", tenantId, request.Domain);
                        return false;
                    }
                }

                // Actualizar propiedades
                tenant.CompanyName = request.CompanyName;
                tenant.Domain = request.Domain;
                tenant.LogoUrl = request.LogoUrl;
                tenant.PrimaryColor = request.PrimaryColor;
                tenant.SecondaryColor = request.SecondaryColor;
                tenant.CustomCss = request.CustomCss;
                tenant.ContactName = request.ContactName;
                tenant.ContactPhone = request.ContactPhone;
                tenant.ContactEmail = request.ContactEmail;
                tenant.BillingEmail = request.BillingEmail;
                tenant.BillingAddress = request.BillingAddress;
                tenant.UpdatedAt = DateTime.UtcNow;

                await _masterContext.SaveChangesAsync();

                // Limpiar cache
                ClearTenantCache(tenantId);

                _logger.LogInformation("Tenant {TenantId} updated successfully", tenantId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tenant {TenantId}", tenantId);
                return false;
            }
        }

        public async Task<bool> ActivateTenantAsync(string tenantId)
        {
            return await UpdateTenantStatusAsync(tenantId, true);
        }

        public async Task<bool> DeactivateTenantAsync(string tenantId)
        {
            return await UpdateTenantStatusAsync(tenantId, false);
        }

        private async Task<bool> UpdateTenantStatusAsync(string tenantId, bool isActive)
        {
            try
            {
                var tenant = await _masterContext.Tenants
                    .FirstOrDefaultAsync(t => t.TenantId == tenantId);

                if (tenant == null)
                {
                    _logger.LogWarning("Attempted to update status of non-existent tenant: {TenantId}", tenantId);
                    return false;
                }

                tenant.IsActive = isActive;
                tenant.UpdatedAt = DateTime.UtcNow;

                await _masterContext.SaveChangesAsync();

                // Limpiar cache
                ClearTenantCache(tenantId);

                _logger.LogInformation("Tenant {TenantId} status updated to {Status}", tenantId, isActive ? "Active" : "Inactive");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tenant {TenantId} status", tenantId);
                return false;
            }
        }

        public async Task<TenantUsageResponse?> GetTenantUsageAsync(string tenantId)
        {
            try
            {
                var tenant = await _masterContext.Tenants
                    .Where(t => t.TenantId == tenantId)
                    .Select(t => new
                    {
                        t.TenantId,
                        t.CompanyName,
                        t.MaxUsers,
                        t.MaxPolizas,
                        t.MonthlyFee,
                        t.LastPaymentDate,
                        t.NextBillingDate,
                        t.SubscriptionPlan
                    })
                    .FirstOrDefaultAsync();

                if (tenant == null)
                    return null;

                // Obtener métricas actuales
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                var metrics = await _masterContext.TenantUsageMetrics
                    .Where(m => m.TenantId == tenantId && m.MetricDate == today)
                    .FirstOrDefaultAsync();

                // Contar usuarios activos
                var currentUsers = await _masterContext.UserTenants
                    .CountAsync(ut => ut.TenantId == tenantId && ut.IsActive);

                return new TenantUsageResponse
                {
                    TenantId = tenant.TenantId,
                    CompanyName = tenant.CompanyName,
                    CurrentUsers = currentUsers,
                    MaxUsers = tenant.MaxUsers,
                    CurrentPolizas = metrics?.TotalPolizas ?? 0,
                    MaxPolizas = tenant.MaxPolizas,
                    StorageUsedMB = metrics?.StorageUsedMB ?? 0,
                    MonthlyFee = tenant.MonthlyFee,
                    LastPaymentDate = tenant.LastPaymentDate,
                    NextBillingDate = tenant.NextBillingDate,
                    SubscriptionPlan = tenant.SubscriptionPlan
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving usage for tenant {TenantId}", tenantId);
                return null;
            }
        }

        public void ClearTenantCache(string tenantId)
        {
            _cache.Remove($"tenant_id_{tenantId}");
            
            // También limpiar cache por dominio si existe
            // Esto es un poco ineficiente, pero asegura consistencia
            var tenant = _masterContext.Tenants
                .Where(t => t.TenantId == tenantId)
                .Select(t => t.Domain)
                .FirstOrDefault();
                
            if (!string.IsNullOrWhiteSpace(tenant))
            {
                _cache.Remove($"tenant_domain_{tenant}");
            }

            _logger.LogDebug("Cache cleared for tenant {TenantId}", tenantId);
        }
    }
}