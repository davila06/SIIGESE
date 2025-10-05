using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Cotizacion : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string NumeroCotizacion { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string NombreSolicitante { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Telefono { get; set; }

        [Required]
        [MaxLength(50)]
        public string TipoSeguro { get; set; } = string.Empty; // AUTO, VIDA, HOGAR, EMPRESARIAL

        [Required]
        [MaxLength(100)]
        public string Aseguradora { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoAsegurado { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? PrimaCotizada { get; set; }

        [Required]
        [MaxLength(10)]
        public string Moneda { get; set; } = "CRC";

        [Required]
        public DateTime FechaCotizacion { get; set; }

        public DateTime? FechaVencimiento { get; set; }

        [Required]
        [MaxLength(20)]
        public string Estado { get; set; } = "PENDIENTE"; // PENDIENTE, APROBADA, RECHAZADA, CONVERTIDA

        [MaxLength(500)]
        public string? Observaciones { get; set; }

        // Datos específicos para seguros de auto
        [MaxLength(20)]
        public string? Placa { get; set; }

        [MaxLength(50)]
        public string? Marca { get; set; }

        [MaxLength(50)]
        public string? Modelo { get; set; }

        public int? Año { get; set; }

        [MaxLength(50)]
        public string? Cilindraje { get; set; }

        // Datos específicos para seguros de vida
        public DateTime? FechaNacimiento { get; set; }

        [MaxLength(20)]
        public string? Genero { get; set; }

        [MaxLength(100)]
        public string? Ocupacion { get; set; }

        // Datos específicos para seguros de hogar
        [MaxLength(200)]
        public string? DireccionInmueble { get; set; }

        [MaxLength(50)]
        public string? TipoInmueble { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ValorInmueble { get; set; }

        // Relación con usuario que creó la cotización
        public int UsuarioId { get; set; }

        // Metadatos
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaActualizacion { get; set; }

        // Propiedades de navegación
        public virtual User Usuario { get; set; } = null!;
    }
}