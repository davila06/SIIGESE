using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.MultiTenant
{
    // Información del tenant actual
    public class TenantInfo
    {
        public string TenantId { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string PrimaryColor { get; set; } = "#1976d2";
        public string SecondaryColor { get; set; } = "#424242";
        public string? CustomCss { get; set; }
        public bool IsActive { get; set; } = true;
        public string SubscriptionPlan { get; set; } = "Basic";
        public int MaxUsers { get; set; } = 10;
        public int MaxPolizas { get; set; } = 1000;
        public string? Domain { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    // Entidad Tenant para Master Database
    public class Tenant
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required, StringLength(50)]
        public string TenantId { get; set; } = string.Empty;
        
        [Required, StringLength(200)]
        public string CompanyName { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? Domain { get; set; }
        
        [Required, StringLength(100)]
        public string DatabaseName { get; set; } = string.Empty;
        
        [Required, StringLength(500)]
        public string ConnectionString { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        
        [Required, StringLength(50)]
        public string SubscriptionPlan { get; set; } = "Basic";
        
        public int MaxUsers { get; set; } = 10;
        public int MaxPolizas { get; set; } = 1000;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Branding
        [StringLength(300)]
        public string? LogoUrl { get; set; }
        
        [StringLength(7)]
        public string PrimaryColor { get; set; } = "#1976d2";
        
        [StringLength(7)]
        public string SecondaryColor { get; set; } = "#424242";
        
        public string? CustomCss { get; set; }
        
        // Facturación
        [StringLength(200)]
        public string? BillingEmail { get; set; }
        
        [StringLength(500)]
        public string? BillingAddress { get; set; }
        
        public DateTime? LastPaymentDate { get; set; }
        public DateTime? NextBillingDate { get; set; }
        public decimal MonthlyFee { get; set; } = 99.99m;
        
        // Contacto
        [StringLength(200)]
        public string? ContactName { get; set; }
        
        [StringLength(50)]
        public string? ContactPhone { get; set; }
        
        [StringLength(200)]
        public string? ContactEmail { get; set; }
        
        // Navegación
        public virtual ICollection<UserTenant> UserTenants { get; set; } = new List<UserTenant>();
        public virtual ICollection<TenantConfiguration> Configurations { get; set; } = new List<TenantConfiguration>();
        public virtual ICollection<TenantUsageMetric> UsageMetrics { get; set; } = new List<TenantUsageMetric>();
        public virtual ICollection<TenantAuditLog> AuditLogs { get; set; } = new List<TenantAuditLog>();
    }

    // Usuario del sistema
    public class SystemUser
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required, StringLength(200)]
        public string Email { get; set; } = string.Empty;
        
        [Required, StringLength(500)]
        public string PasswordHash { get; set; } = string.Empty;
        
        [Required, StringLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required, StringLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        public bool IsSuperAdmin { get; set; } = false;
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        
        // Navegación
        public virtual ICollection<UserTenant> UserTenants { get; set; } = new List<UserTenant>();
        public virtual ICollection<TenantAuditLog> AuditLogs { get; set; } = new List<TenantAuditLog>();
        
        // Computed Properties
        public string FullName => $"{FirstName} {LastName}";
    }

    // Relación Usuario-Tenant
    public class UserTenant
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid UserId { get; set; }
        
        [Required, StringLength(50)]
        public string TenantId { get; set; } = string.Empty;
        
        [Required, StringLength(50)]
        public string Role { get; set; } = "User"; // SuperAdmin, Admin, Manager, User, ReadOnly
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid? CreatedByUserId { get; set; }
        
        // Navegación
        public virtual SystemUser User { get; set; } = null!;
        public virtual Tenant Tenant { get; set; } = null!;
    }

    // Configuraciones por tenant
    public class TenantConfiguration
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required, StringLength(50)]
        public string TenantId { get; set; } = string.Empty;
        
        [Required, StringLength(100)]
        public string ConfigKey { get; set; } = string.Empty;
        
        public string? ConfigValue { get; set; }
        
        [StringLength(50)]
        public string ConfigType { get; set; } = "String"; // String, Number, Boolean, JSON
        
        public bool IsEncrypted { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Navegación
        public virtual Tenant Tenant { get; set; } = null!;
    }

    // Auditoría por tenant
    public class TenantAuditLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required, StringLength(50)]
        public string TenantId { get; set; } = string.Empty;
        
        public Guid? UserId { get; set; }
        
        [Required, StringLength(100)]
        public string Action { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? EntityType { get; set; }
        
        [StringLength(100)]
        public string? EntityId { get; set; }
        
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        
        [StringLength(45)]
        public string? IPAddress { get; set; }
        
        [StringLength(500)]
        public string? UserAgent { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navegación
        public virtual Tenant Tenant { get; set; } = null!;
        public virtual SystemUser? User { get; set; }
    }

    // Métricas de uso por tenant
    public class TenantUsageMetric
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required, StringLength(50)]
        public string TenantId { get; set; } = string.Empty;
        
        public DateOnly MetricDate { get; set; }
        
        public int ActiveUsers { get; set; } = 0;
        public int TotalPolizas { get; set; } = 0;
        public int TotalCobros { get; set; } = 0;
        public int TotalReclamos { get; set; } = 0;
        public decimal StorageUsedMB { get; set; } = 0;
        public int ApiCallsCount { get; set; } = 0;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navegación
        public virtual Tenant Tenant { get; set; } = null!;
    }

    // DTOs para API
    public class CreateTenantRequest
    {
        [Required, StringLength(50)]
        public string TenantId { get; set; } = string.Empty;
        
        [Required, StringLength(200)]
        public string CompanyName { get; set; } = string.Empty;
        
        [Required, EmailAddress, StringLength(200)]
        public string AdminEmail { get; set; } = string.Empty;
        
        [Required, StringLength(100)]
        public string AdminFirstName { get; set; } = string.Empty;
        
        [Required, StringLength(100)]
        public string AdminLastName { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string SubscriptionPlan { get; set; } = "Basic";
        
        [StringLength(100)]
        public string? Domain { get; set; }
        
        [StringLength(300)]
        public string? LogoUrl { get; set; }
        
        [StringLength(7)]
        public string PrimaryColor { get; set; } = "#1976d2";
        
        [StringLength(7)]
        public string SecondaryColor { get; set; } = "#424242";
    }

    public class UpdateTenantRequest
    {
        [Required, StringLength(200)]
        public string CompanyName { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? Domain { get; set; }
        
        [StringLength(300)]
        public string? LogoUrl { get; set; }
        
        [StringLength(7)]
        public string PrimaryColor { get; set; } = "#1976d2";
        
        [StringLength(7)]
        public string SecondaryColor { get; set; } = "#424242";
        
        public string? CustomCss { get; set; }
        
        [StringLength(200)]
        public string? ContactName { get; set; }
        
        [StringLength(50)]
        public string? ContactPhone { get; set; }
        
        [EmailAddress, StringLength(200)]
        public string? ContactEmail { get; set; }
        
        [EmailAddress, StringLength(200)]
        public string? BillingEmail { get; set; }
        
        [StringLength(500)]
        public string? BillingAddress { get; set; }
    }

    public class TenantResponse
    {
        public string TenantId { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? Domain { get; set; }
        public string? LogoUrl { get; set; }
        public string PrimaryColor { get; set; } = "#1976d2";
        public string SecondaryColor { get; set; } = "#424242";
        public bool IsActive { get; set; }
        public string SubscriptionPlan { get; set; } = "Basic";
        public int MaxUsers { get; set; }
        public int MaxPolizas { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ContactName { get; set; }
        public string? ContactEmail { get; set; }
    }

    public class UserTenantResponse
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TenantUsageResponse
    {
        public string TenantId { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public int CurrentUsers { get; set; }
        public int MaxUsers { get; set; }
        public int CurrentPolizas { get; set; }
        public int MaxPolizas { get; set; }
        public decimal StorageUsedMB { get; set; }
        public decimal MonthlyFee { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public DateTime? NextBillingDate { get; set; }
        public string SubscriptionPlan { get; set; } = string.Empty;
    }

    // Enums
    public enum TenantRole
    {
        SuperAdmin,
        Admin,
        Manager,
        User,
        ReadOnly
    }

    public enum SubscriptionPlan
    {
        Basic,
        Professional,
        Enterprise
    }

    public enum ConfigType
    {
        String,
        Number,
        Boolean,
        JSON
    }
}