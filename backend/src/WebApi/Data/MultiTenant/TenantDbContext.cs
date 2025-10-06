using Microsoft.EntityFrameworkCore;
using WebApi.Models.MultiTenant;
using WebApi.Middleware;

namespace WebApi.Data.MultiTenant
{
    /// <summary>
    /// DbContext dinámico que se conecta a la base de datos del tenant actual
    /// </summary>
    public class TenantDbContext : DbContext
    {
        private readonly ITenantContext _tenantContext;
        private readonly IConfiguration _configuration;

        public TenantDbContext(
            DbContextOptions<TenantDbContext> options,
            ITenantContext tenantContext,
            IConfiguration configuration) : base(options)
        {
            _tenantContext = tenantContext;
            _configuration = configuration;
        }

        // Mismas entidades que el DbContext original
        public DbSet<Domain.Entities.Poliza> Polizas { get; set; }
        public DbSet<Domain.Entities.Cobro> Cobros { get; set; }
        public DbSet<Domain.Entities.Reclamo> Reclamos { get; set; }
        public DbSet<Domain.Entities.User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = GetTenantConnectionString();
                if (!string.IsNullOrEmpty(connectionString))
                {
                    optionsBuilder.UseSqlServer(connectionString, options =>
                    {
                        options.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorNumbersToAdd: null);
                    });
                }
                else
                {
                    // Fallback a conexión por defecto
                    var defaultConnection = _configuration.GetConnectionString("DefaultConnection");
                    optionsBuilder.UseSqlServer(defaultConnection);
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aplicar las mismas configuraciones que el DbContext original
            // Esto debería ser idéntico a tu ApplicationDbContext existente

            // Ejemplo de configuración de entidades
            ConfigurePolizaEntity(modelBuilder);
            ConfigureCobroEntity(modelBuilder);
            ConfigureReclamoEntity(modelBuilder);
            ConfigureUserEntity(modelBuilder);
        }

        private void ConfigurePolizaEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Domain.Entities.Poliza>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NumeroPoliza).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ClienteNombre).HasMaxLength(200);
                entity.Property(e => e.ClienteApellido).HasMaxLength(200);
                entity.Property(e => e.ClienteDni).HasMaxLength(20);
                entity.Property(e => e.VigenciaDesde).IsRequired();
                entity.Property(e => e.VigenciaHasta).IsRequired();
                entity.Property(e => e.Premio).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Estado).HasMaxLength(50).HasDefaultValue("Activa");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(e => e.NumeroPoliza).IsUnique();
                entity.HasIndex(e => e.ClienteDni);
                entity.HasIndex(e => e.VigenciaDesde);
                entity.HasIndex(e => e.VigenciaHasta);
            });
        }

        private void ConfigureCobroEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Domain.Entities.Cobro>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NumeroPoliza).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ClienteNombre).HasMaxLength(200);
                entity.Property(e => e.ClienteApellido).HasMaxLength(200);
                entity.Property(e => e.MontoTotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.MontoPagado).HasColumnType("decimal(18,2)");
                entity.Property(e => e.FechaVencimiento).IsRequired();
                entity.Property(e => e.Estado).HasMaxLength(50).HasDefaultValue("Pendiente");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(e => e.NumeroPoliza);
                entity.HasIndex(e => e.FechaVencimiento);
                entity.HasIndex(e => e.Estado);
            });
        }

        private void ConfigureReclamoEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Domain.Entities.Reclamo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NumeroReclamo).IsRequired().HasMaxLength(50);
                entity.Property(e => e.NumeroPoliza).HasMaxLength(50);
                entity.Property(e => e.ClienteNombre).HasMaxLength(200);
                entity.Property(e => e.ClienteApellido).HasMaxLength(200);
                entity.Property(e => e.TipoReclamo).HasMaxLength(100);
                entity.Property(e => e.Estado).HasMaxLength(50).HasDefaultValue("Pendiente");
                entity.Property(e => e.MontoReclamado).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(e => e.NumeroReclamo).IsUnique();
                entity.HasIndex(e => e.NumeroPoliza);
                entity.HasIndex(e => e.Estado);
            });
        }

        private void ConfigureUserEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Domain.Entities.User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Role).HasMaxLength(50).HasDefaultValue("User");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Role);
            });
        }

        private string? GetTenantConnectionString()
        {
            // Obtener connection string del tenant actual
            var tenantInfo = _tenantContext.CurrentTenant;
            return tenantInfo?.ConnectionString;
        }

        /// <summary>
        /// Override para interceptar operaciones y agregar auditoría
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Agregar información de auditoría antes de guardar
            AddAuditInformation();
            
            return await base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            AddAuditInformation();
            return base.SaveChanges();
        }

        private void AddAuditInformation()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                // Agregar timestamps automáticamente
                if (entry.State == EntityState.Added)
                {
                    if (entry.Entity.GetType().GetProperty("CreatedAt") != null)
                    {
                        entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
                    }
                }

                if (entry.State == EntityState.Modified)
                {
                    if (entry.Entity.GetType().GetProperty("UpdatedAt") != null)
                    {
                        entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
                    }
                }

                // Agregar información del tenant si la entidad la soporta
                if (entry.Entity.GetType().GetProperty("TenantId") != null && entry.State == EntityState.Added)
                {
                    var tenantId = _tenantContext.TenantId;
                    if (!string.IsNullOrEmpty(tenantId))
                    {
                        entry.Property("TenantId").CurrentValue = tenantId;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Factory para crear TenantDbContext con configuración dinámica
    /// </summary>
    public class TenantDbContextFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public TenantDbContextFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public TenantDbContext CreateDbContext(string tenantId)
        {
            using var scope = _serviceProvider.CreateScope();
            var tenantService = scope.ServiceProvider.GetRequiredService<ITenantService>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            
            var tenantInfo = tenantService.GetTenantByIdAsync(tenantId).GetAwaiter().GetResult();
            
            if (tenantInfo == null)
                throw new InvalidOperationException($"Tenant '{tenantId}' not found");

            var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();
            optionsBuilder.UseSqlServer(tenantInfo.ConnectionString);

            // Crear un TenantContext mock para este tenant específico
            var mockTenantContext = new MockTenantContext(tenantInfo);
            
            return new TenantDbContext(optionsBuilder.Options, mockTenantContext, configuration);
        }
    }

    /// <summary>
    /// Mock TenantContext para uso en factory
    /// </summary>
    internal class MockTenantContext : ITenantContext
    {
        public MockTenantContext(TenantInfo tenantInfo)
        {
            CurrentTenant = tenantInfo;
            TenantId = tenantInfo.TenantId;
            HasTenant = true;
        }

        public TenantInfo? CurrentTenant { get; }
        public string? TenantId { get; }
        public bool HasTenant { get; }
    }
}