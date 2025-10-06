using Microsoft.EntityFrameworkCore;
using WebApi.Models.MultiTenant;

namespace WebApi.Data.MultiTenant
{
    /// <summary>
    /// DbContext para la Master Database que contiene información de todos los tenants
    /// </summary>
    public class MasterDbContext : DbContext
    {
        public MasterDbContext(DbContextOptions<MasterDbContext> options) : base(options)
        {
        }

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<SystemUser> SystemUsers { get; set; }
        public DbSet<UserTenant> UserTenants { get; set; }
        public DbSet<TenantConfiguration> TenantConfigurations { get; set; }
        public DbSet<TenantAuditLog> TenantAuditLogs { get; set; }
        public DbSet<TenantUsageMetric> TenantUsageMetrics { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de Tenant
            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.TenantId).IsUnique();
                entity.HasIndex(e => e.Domain).IsUnique();
                entity.HasIndex(e => e.IsActive);
                
                entity.Property(e => e.TenantId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CompanyName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.DatabaseName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ConnectionString).IsRequired().HasMaxLength(500);
                entity.Property(e => e.SubscriptionPlan).IsRequired().HasMaxLength(50).HasDefaultValue("Basic");
                entity.Property(e => e.PrimaryColor).HasMaxLength(7).HasDefaultValue("#1976d2");
                entity.Property(e => e.SecondaryColor).HasMaxLength(7).HasDefaultValue("#424242");
                entity.Property(e => e.MaxUsers).HasDefaultValue(10);
                entity.Property(e => e.MaxPolizas).HasDefaultValue(1000);
                entity.Property(e => e.MonthlyFee).HasColumnType("decimal(10,2)").HasDefaultValue(99.99m);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // Configuración de SystemUser
            modelBuilder.Entity<SystemUser>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.IsActive);
                
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(500);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // Configuración de UserTenant
            modelBuilder.Entity<UserTenant>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UserId, e.TenantId }).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.TenantId);
                entity.HasIndex(e => e.Role);
                
                entity.Property(e => e.TenantId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(50).HasDefaultValue("User");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                entity.HasOne(e => e.User)
                    .WithMany(u => u.UserTenants)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.Tenant)
                    .WithMany(t => t.UserTenants)
                    .HasForeignKey(e => e.TenantId)
                    .HasPrincipalKey(t => t.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de TenantConfiguration
            modelBuilder.Entity<TenantConfiguration>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.ConfigKey }).IsUnique();
                entity.HasIndex(e => e.TenantId);
                entity.HasIndex(e => e.ConfigKey);
                
                entity.Property(e => e.TenantId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ConfigKey).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ConfigType).HasMaxLength(50).HasDefaultValue("String");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                entity.HasOne(e => e.Tenant)
                    .WithMany(t => t.Configurations)
                    .HasForeignKey(e => e.TenantId)
                    .HasPrincipalKey(t => t.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de TenantAuditLog
            modelBuilder.Entity<TenantAuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.TenantId);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.Action);
                
                entity.Property(e => e.TenantId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
                entity.Property(e => e.EntityType).HasMaxLength(100);
                entity.Property(e => e.EntityId).HasMaxLength(100);
                entity.Property(e => e.IPAddress).HasMaxLength(45);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                entity.HasOne(e => e.Tenant)
                    .WithMany(t => t.AuditLogs)
                    .HasForeignKey(e => e.TenantId)
                    .HasPrincipalKey(t => t.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.User)
                    .WithMany(u => u.AuditLogs)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configuración de TenantUsageMetric
            modelBuilder.Entity<TenantUsageMetric>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.MetricDate }).IsUnique();
                entity.HasIndex(e => e.TenantId);
                entity.HasIndex(e => e.MetricDate);
                
                entity.Property(e => e.TenantId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.StorageUsedMB).HasColumnType("decimal(10,2)");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                entity.HasOne(e => e.Tenant)
                    .WithMany(t => t.UsageMetrics)
                    .HasForeignKey(e => e.TenantId)
                    .HasPrincipalKey(t => t.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Seed data
            SeedInitialData(modelBuilder);
        }

        private void SeedInitialData(ModelBuilder modelBuilder)
        {
            // Tenant por defecto
            var defaultTenantId = "empresa-default";
            var defaultTenant = new Tenant
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                TenantId = defaultTenantId,
                CompanyName = "Mi Empresa Default",
                DatabaseName = "SinsegTenant_Default",
                ConnectionString = "Server=(localdb)\\mssqllocaldb;Database=SinsegTenant_Default;Trusted_Connection=true;MultipleActiveResultSets=true",
                SubscriptionPlan = "Professional",
                MaxUsers = 50,
                MaxPolizas = 5000,
                BillingEmail = "admin@miempresa.com",
                ContactName = "Administrador",
                ContactEmail = "admin@miempresa.com",
                PrimaryColor = "#1976d2",
                SecondaryColor = "#424242",
                CreatedAt = DateTime.UtcNow
            };

            // Super Admin
            var superAdminId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var superAdmin = new SystemUser
            {
                Id = superAdminId,
                Email = "superadmin@siinadseg.com",
                PasswordHash = "$2a$11$wK0FZHR6o.l1Q8q8dXmQ7uWJ9u5HJV8uJEXFhVz7Q8YWOzQU6J6nK", // Admin@123
                FirstName = "Super",
                LastName = "Admin",
                IsSuperAdmin = true,
                CreatedAt = DateTime.UtcNow
            };

            // Relación Super Admin - Tenant Default
            var userTenant = new UserTenant
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                UserId = superAdminId,
                TenantId = defaultTenantId,
                Role = "SuperAdmin",
                CreatedAt = DateTime.UtcNow
            };

            // Configuraciones iniciales
            var configurations = new[]
            {
                new TenantConfiguration
                {
                    Id = Guid.NewGuid(),
                    TenantId = defaultTenantId,
                    ConfigKey = "SMTP_SERVER",
                    ConfigValue = "smtp.gmail.com",
                    ConfigType = "String",
                    CreatedAt = DateTime.UtcNow
                },
                new TenantConfiguration
                {
                    Id = Guid.NewGuid(),
                    TenantId = defaultTenantId,
                    ConfigKey = "SMTP_PORT",
                    ConfigValue = "587",
                    ConfigType = "Number",
                    CreatedAt = DateTime.UtcNow
                },
                new TenantConfiguration
                {
                    Id = Guid.NewGuid(),
                    TenantId = defaultTenantId,
                    ConfigKey = "TIMEZONE",
                    ConfigValue = "America/Argentina/Buenos_Aires",
                    ConfigType = "String",
                    CreatedAt = DateTime.UtcNow
                },
                new TenantConfiguration
                {
                    Id = Guid.NewGuid(),
                    TenantId = defaultTenantId,
                    ConfigKey = "CURRENCY",
                    ConfigValue = "ARS",
                    ConfigType = "String",
                    CreatedAt = DateTime.UtcNow
                },
                new TenantConfiguration
                {
                    Id = Guid.NewGuid(),
                    TenantId = defaultTenantId,
                    ConfigKey = "LANGUAGE",
                    ConfigValue = "es-AR",
                    ConfigType = "String",
                    CreatedAt = DateTime.UtcNow
                },
                new TenantConfiguration
                {
                    Id = Guid.NewGuid(),
                    TenantId = defaultTenantId,
                    ConfigKey = "ENABLE_NOTIFICATIONS",
                    ConfigValue = "true",
                    ConfigType = "Boolean",
                    CreatedAt = DateTime.UtcNow
                }
            };

            // Aplicar seed data
            modelBuilder.Entity<Tenant>().HasData(defaultTenant);
            modelBuilder.Entity<SystemUser>().HasData(superAdmin);
            modelBuilder.Entity<UserTenant>().HasData(userTenant);
            modelBuilder.Entity<TenantConfiguration>().HasData(configurations);
        }

        /// <summary>
        /// Configurar las propiedades automáticas antes de guardar
        /// </summary>
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        /// <summary>
        /// Configurar las propiedades automáticas antes de guardar (async)
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.Entity is Tenant tenant)
                {
                    tenant.UpdatedAt = DateTime.UtcNow;
                }
                else if (entry.Entity is TenantConfiguration config)
                {
                    config.UpdatedAt = DateTime.UtcNow;
                }
            }
        }
    }
}