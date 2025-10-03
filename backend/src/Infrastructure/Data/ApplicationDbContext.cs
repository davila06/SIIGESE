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