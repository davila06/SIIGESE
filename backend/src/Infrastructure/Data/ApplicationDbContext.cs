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
        public DbSet<Cobro> Cobros { get; set; }
        public DbSet<Reclamo> Reclamos { get; set; }
        public DbSet<Cotizacion> Cotizaciones { get; set; }
        public DbSet<EmailConfig> EmailConfigs { get; set; }
        public DbSet<EmailLog> EmailLogs { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<ChatSession> ChatSessions { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<ReclamoHistorial> ReclamoHistoriales { get; set; }
        public DbSet<CobroEstadoChangeRequest> CobroEstadoChangeRequests { get; set; }

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
                entity.Property(e => e.FileType).HasMaxLength(100);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);

                entity.HasOne(e => e.UploadedBy)
                    .WithMany()
                    .HasForeignKey(e => e.UploadedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // PasswordResetToken configuration
            modelBuilder.Entity<PasswordResetToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Token).IsRequired();
                entity.HasIndex(e => e.ExpiresAt);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Keep this aligned with User soft-delete query filter.
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // Cotizacion configuration
            modelBuilder.Entity<Cotizacion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Prima).HasPrecision(18, 2);

                entity.HasOne(e => e.Usuario)
                    .WithMany()
                    .HasForeignKey(e => e.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Keep this aligned with User soft-delete query filter.
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // Cobro configuration
            modelBuilder.Entity<Cobro>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MontoTotal).HasPrecision(18, 2);
                entity.Property(e => e.MontoCobrado).HasPrecision(18, 2);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            modelBuilder.Entity<CobroEstadoChangeRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MotivoSolicitud).HasMaxLength(1000);
                entity.Property(e => e.MotivoDecision).HasMaxLength(1000);
                entity.Property(e => e.SolicitadoPorNombre).IsRequired().HasMaxLength(200);
                entity.Property(e => e.SolicitadoPorEmail).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ResueltoPorNombre).HasMaxLength(200);

                entity.HasIndex(e => e.CobroId);
                entity.HasIndex(e => e.SolicitadoPorUserId);
                entity.HasIndex(e => e.EstadoSolicitud);
                entity.HasIndex(e => e.CreatedAt);

                entity.HasOne(e => e.Cobro)
                    .WithMany()
                    .HasForeignKey(e => e.CobroId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // Reclamo configuration
            modelBuilder.Entity<Reclamo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MontoReclamado).HasPrecision(18, 2);
                entity.Property(e => e.MontoAprobado).HasPrecision(18, 2);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // EmailLog configuration
            modelBuilder.Entity<EmailLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ToEmail).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ToName).HasMaxLength(200);
                entity.Property(e => e.Subject).IsRequired().HasMaxLength(250);
                entity.Property(e => e.EmailType).HasMaxLength(50);
                entity.Property(e => e.SenderName).HasMaxLength(100);
                entity.Property(e => e.ErrorMessage).HasMaxLength(1000);

                entity.HasIndex(e => e.SentAt);
                entity.HasIndex(e => e.IsSuccess);
                entity.HasIndex(e => e.EmailType);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // Poliza configuration
            modelBuilder.Entity<Poliza>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NumeroPoliza).HasMaxLength(50);
                entity.Property(e => e.Modalidad).HasMaxLength(50);
                entity.Property(e => e.NombreAsegurado).HasMaxLength(200);
                entity.Property(e => e.NumeroCedula).HasMaxLength(50);
                entity.Property(e => e.Prima).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Moneda).HasMaxLength(3);
                entity.Property(e => e.Frecuencia).HasMaxLength(50);
                entity.Property(e => e.Aseguradora).HasMaxLength(100);
                entity.Property(e => e.Placa).HasMaxLength(10);
                entity.Property(e => e.Marca).HasMaxLength(50);
                entity.Property(e => e.Modelo).HasMaxLength(50);
                entity.Property(e => e.Año).HasMaxLength(4);
                entity.Property(e => e.Correo).HasMaxLength(100);
                entity.Property(e => e.NumeroTelefono).HasMaxLength(20);

                entity.HasIndex(e => e.NumeroPoliza);
                entity.HasIndex(e => e.Placa);
                entity.HasIndex(e => e.PerfilId);
                entity.HasIndex(e => e.Aseguradora);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // ReclamoHistorial configuration
            modelBuilder.Entity<ReclamoHistorial>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TipoEvento).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ValorAnterior).HasMaxLength(200);
                entity.Property(e => e.ValorNuevo).HasMaxLength(200);
                entity.Property(e => e.Descripcion).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Usuario).IsRequired().HasMaxLength(100);

                entity.HasIndex(e => e.ReclamoId);
                entity.HasIndex(e => e.CreatedAt);

                entity.HasOne(e => e.Reclamo)
                    .WithMany()
                    .HasForeignKey(e => e.ReclamoId)
                    .OnDelete(DeleteBehavior.Cascade);

                // History entries are immutable — global query filter only hides soft-deleted rows.
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // ChatSession configuration
            modelBuilder.Entity<ChatSession>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SessionId).IsRequired().HasMaxLength(64);
                entity.Property(e => e.Title).HasMaxLength(200);
                entity.Property(e => e.LastMessage).HasMaxLength(300);
                entity.HasIndex(e => e.SessionId).IsUnique();
                entity.HasIndex(e => e.UserId);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // ChatMessage configuration
            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).IsRequired().HasMaxLength(2000);
                entity.HasIndex(e => e.ChatSessionId);

                entity.HasOne(e => e.ChatSession)
                    .WithMany(s => s.Messages)
                    .HasForeignKey(e => e.ChatSessionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasQueryFilter(e => !e.IsDeleted);
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