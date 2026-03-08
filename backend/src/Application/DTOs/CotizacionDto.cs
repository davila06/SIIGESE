using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class CreateCotizacionDto
    {
        [Required(ErrorMessage = "El nombre del solicitante es obligatorio")]
        [MaxLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        public string NombreSolicitante { get; set; } = string.Empty;

        [MaxLength(200, ErrorMessage = "El nombre del asegurado no puede exceder 200 caracteres")]
        public string? NombreAsegurado { get; set; }

        [MaxLength(20, ErrorMessage = "La cédula no puede exceder 20 caracteres")]
        public string? NumeroCedula { get; set; }

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [MaxLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
        public string? Telefono { get; set; }

        [MaxLength(100, ErrorMessage = "El correo no puede exceder 100 caracteres")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public string? Correo { get; set; }

        [Required(ErrorMessage = "El tipo de seguro es obligatorio")]
        [MaxLength(50, ErrorMessage = "El tipo de seguro no puede exceder 50 caracteres")]
        public string TipoSeguro { get; set; } = string.Empty;

        [MaxLength(50, ErrorMessage = "La modalidad no puede exceder 50 caracteres")]
        public string? Modalidad { get; set; }

        [MaxLength(50, ErrorMessage = "La frecuencia no puede exceder 50 caracteres")]
        public string? Frecuencia { get; set; }

        [Required(ErrorMessage = "La aseguradora es obligatoria")]
        [MaxLength(200, ErrorMessage = "La aseguradora no puede exceder 200 caracteres")]
        public string Aseguradora { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto asegurado es obligatorio")]
        [Range(1, double.MaxValue, ErrorMessage = "El monto asegurado debe ser mayor a 0")]
        public decimal MontoAsegurado { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "La prima debe ser mayor o igual a 0")]
        public decimal? PrimaCotizada { get; set; }

        [Required(ErrorMessage = "La moneda es obligatoria")]
        [MaxLength(10, ErrorMessage = "La moneda no puede exceder 10 caracteres")]
        public string Moneda { get; set; } = "CRC";

        public DateTime? FechaVencimiento { get; set; }

        [MaxLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
        public string? Observaciones { get; set; }

        // Datos específicos para seguros de auto
        [MaxLength(20, ErrorMessage = "La placa no puede exceder 20 caracteres")]
        public string? Placa { get; set; }

        [MaxLength(50, ErrorMessage = "La marca no puede exceder 50 caracteres")]
        public string? Marca { get; set; }

        [MaxLength(50, ErrorMessage = "El modelo no puede exceder 50 caracteres")]
        public string? Modelo { get; set; }

        [Range(1900, 2100, ErrorMessage = "Año inválido")]
        public int? Año { get; set; }

        [MaxLength(50, ErrorMessage = "El cilindraje no puede exceder 50 caracteres")]
        public string? Cilindraje { get; set; }

        // Datos específicos para seguros de vida
        public DateTime? FechaNacimiento { get; set; }

        [MaxLength(20, ErrorMessage = "El género no puede exceder 20 caracteres")]
        public string? Genero { get; set; }

        [MaxLength(100, ErrorMessage = "La ocupación no puede exceder 100 caracteres")]
        public string? Ocupacion { get; set; }

        // Datos específicos para seguros de hogar
        [MaxLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres")]
        public string? DireccionInmueble { get; set; }

        public int? PerfilId { get; set; }

        [MaxLength(50, ErrorMessage = "El tipo de inmueble no puede exceder 50 caracteres")]
        public string? TipoInmueble { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El valor del inmueble debe ser mayor o igual a 0")]
        public decimal? ValorInmueble { get; set; }
    }

    public class UpdateCotizacionDto : CreateCotizacionDto
    {
        [MaxLength(20, ErrorMessage = "El estado no puede exceder 20 caracteres")]
        public string? Estado { get; set; }
    }

    public class CotizacionDto
    {
        public int Id { get; set; }
        public string NumeroCotizacion { get; set; } = string.Empty;
        public string NombreSolicitante { get; set; } = string.Empty;
        public string? NombreAsegurado { get; set; }
        public string? NumeroCedula { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
        public string TipoSeguro { get; set; } = string.Empty;
        public string? Modalidad { get; set; }
        public string? Frecuencia { get; set; }
        public string Aseguradora { get; set; } = string.Empty;
        public decimal MontoAsegurado { get; set; }
        public decimal? PrimaCotizada { get; set; }
        public string Moneda { get; set; } = string.Empty;
        public DateTime FechaCotizacion { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
        public int? PerfilId { get; set; }

        // Datos específicos para seguros de auto
        public string? Placa { get; set; }
        public string? Marca { get; set; }
        public string? Modelo { get; set; }
        public int? Año { get; set; }
        public string? Cilindraje { get; set; }

        // Datos específicos para seguros de vida
        public DateTime? FechaNacimiento { get; set; }
        public string? Genero { get; set; }
        public string? Ocupacion { get; set; }

        // Datos específicos para seguros de hogar
        public string? DireccionInmueble { get; set; }
        public string? TipoInmueble { get; set; }
        public decimal? ValorInmueble { get; set; }

        public int UsuarioId { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
    }

    public class CotizacionSearchDto
    {
        public string? NumeroCotizacion { get; set; }
        public string? NombreSolicitante { get; set; }
        public string? Email { get; set; }
        public string? TipoSeguro { get; set; }
        public string? Estado { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}