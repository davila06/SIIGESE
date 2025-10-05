using Microsoft.EntityFrameworkCore;
using Domain.Entities;

namespace Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<DataRecord> DataRecords { get; set; }
        public DbSet<Poliza> Polizas { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<Cobro> Cobros { get; set; }
        public DbSet<Cotizacion> Cotizaciones { get; set; }
        public DbSet<Reclamo> Reclamos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.UserName).IsUnique();

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // Role configuration
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(200);
                entity.HasIndex(e => e.Name).IsUnique();

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // UserRole configuration
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UserId, e.RoleId }).IsUnique();

                entity.HasOne(e => e.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // Cliente configuration
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Codigo).IsRequired().HasMaxLength(20);
                entity.Property(e => e.RazonSocial).IsRequired().HasMaxLength(200);
                entity.Property(e => e.NombreComercial).HasMaxLength(200);
                entity.Property(e => e.NIT).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Telefono).HasMaxLength(20);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Direccion).HasMaxLength(300);
                entity.Property(e => e.Ciudad).HasMaxLength(100);
                entity.Property(e => e.Departamento).HasMaxLength(100);
                entity.Property(e => e.Pais).HasMaxLength(100);

                entity.HasIndex(e => e.Codigo).IsUnique();
                entity.HasIndex(e => e.NIT).IsUnique();
                entity.HasIndex(e => e.PerfilId);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // DataRecord configuration
            modelBuilder.Entity<DataRecord>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FileType).HasMaxLength(100); // Aumentado para soportar tipos MIME largos
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);

                entity.HasOne(e => e.UploadedBy)
                    .WithMany()
                    .HasForeignKey(e => e.UploadedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // Poliza configuration
            modelBuilder.Entity<Poliza>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NumeroPoliza).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Modalidad).HasMaxLength(50);
                entity.Property(e => e.NombreAsegurado).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Prima).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Moneda).IsRequired().HasMaxLength(3);
                entity.Property(e => e.Frecuencia).HasMaxLength(50);
                entity.Property(e => e.Aseguradora).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Placa).HasMaxLength(10);
                entity.Property(e => e.Marca).HasMaxLength(50);
                entity.Property(e => e.Modelo).HasMaxLength(50);

                entity.HasIndex(e => e.NumeroPoliza).IsUnique();
                entity.HasIndex(e => e.Placa);
                entity.HasIndex(e => e.PerfilId);
                entity.HasIndex(e => e.Aseguradora);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // PasswordResetToken configuration
            modelBuilder.Entity<PasswordResetToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Token).IsRequired().HasMaxLength(255);
                entity.Property(e => e.IpAddress).HasMaxLength(45); // IPv6 support
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                
                entity.HasIndex(e => e.Token).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.ExpiresAt);
                
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // Cobro configuration
            modelBuilder.Entity<Cobro>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NumeroRecibo).IsRequired().HasMaxLength(50);
                entity.Property(e => e.NumeroPoliza).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ClienteNombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ClienteApellido).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FechaVencimiento).IsRequired();
                entity.Property(e => e.MontoTotal).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.MontoCobrado).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Estado).IsRequired();
                entity.Property(e => e.Observaciones).HasMaxLength(500);
                entity.Property(e => e.UsuarioCobroNombre).HasMaxLength(100);

                entity.HasIndex(e => e.NumeroRecibo).IsUnique();
                entity.HasIndex(e => e.PolizaId);
                entity.HasIndex(e => e.Estado);
                entity.HasIndex(e => e.FechaVencimiento);

                entity.HasOne(e => e.Poliza)
                      .WithMany()
                      .HasForeignKey(e => e.PolizaId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.UsuarioCobro)
                      .WithMany()
                      .HasForeignKey(e => e.UsuarioCobroId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // Cotizacion configuration
            modelBuilder.Entity<Cotizacion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NumeroCotizacion).IsRequired().HasMaxLength(50);
                entity.Property(e => e.NombreSolicitante).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Telefono).HasMaxLength(20);
                entity.Property(e => e.TipoSeguro).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Aseguradora).IsRequired().HasMaxLength(100);
                entity.Property(e => e.MontoAsegurado).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.PrimaCotizada).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Moneda).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Estado).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Observaciones).HasMaxLength(500);

                // Campos específicos para auto
                entity.Property(e => e.Placa).HasMaxLength(20);
                entity.Property(e => e.Marca).HasMaxLength(50);
                entity.Property(e => e.Modelo).HasMaxLength(50);
                entity.Property(e => e.Cilindraje).HasMaxLength(50);

                // Campos específicos para vida
                entity.Property(e => e.Genero).HasMaxLength(20);
                entity.Property(e => e.Ocupacion).HasMaxLength(100);

                // Campos específicos para hogar
                entity.Property(e => e.DireccionInmueble).HasMaxLength(200);
                entity.Property(e => e.TipoInmueble).HasMaxLength(50);
                entity.Property(e => e.ValorInmueble).HasColumnType("decimal(18,2)");

                entity.HasIndex(e => e.NumeroCotizacion).IsUnique();
                entity.HasIndex(e => e.Email);
                entity.HasIndex(e => e.TipoSeguro);
                entity.HasIndex(e => e.Estado);
                entity.HasIndex(e => e.FechaCotizacion);

                entity.HasOne(e => e.Usuario)
                      .WithMany()
                      .HasForeignKey(e => e.UsuarioId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin", Description = "Administrador del sistema", CreatedBy = "System" },
                new Role { Id = 2, Name = "DataLoader", Description = "Cargador de datos", CreatedBy = "System" },
                new Role { Id = 3, Name = "User", Description = "Usuario estándar", CreatedBy = "System" }
            );

            // Seed Admin User
            modelBuilder.Entity<User>().HasData(
                new User 
                { 
                    Id = 1, 
                    UserName = "admin", 
                    Email = "admin@sinseg.com",
                    FirstName = "Administrador",
                    LastName = "Sistema",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    RequiresPasswordChange = false, // Admin no requiere cambio inicial
                    LastPasswordChangeAt = DateTime.UtcNow,
                    CreatedBy = "System"
                }
            );

            // Seed Admin UserRole
            modelBuilder.Entity<UserRole>().HasData(
                new UserRole { Id = 1, UserId = 1, RoleId = 1, CreatedBy = "System" }
            );
        }
    }
}