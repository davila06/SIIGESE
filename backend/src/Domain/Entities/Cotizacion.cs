using System;

namespace Domain.Entities
{
    public class Cotizacion : Entity
    {
        public string NumeroCotizacion { get; set; } = string.Empty;
        public string NumeroPoliza { get; set; } = string.Empty;
        public string NombreAsegurado { get; set; } = string.Empty;
        public string NombreSolicitante { get; set; } = string.Empty;
        public string NumeroCedula { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NumeroTelefono { get; set; } = string.Empty;
        public string TipoSeguro { get; set; } = string.Empty;
        public string Modalidad { get; set; } = string.Empty;
        public decimal Prima { get; set; }
        public string Moneda { get; set; } = string.Empty;
        public DateTime FechaVigencia { get; set; }
        public string Frecuencia { get; set; } = string.Empty;
        public DateTime FechaCotizacion { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public string Aseguradora { get; set; } = string.Empty;
        public string Placa { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Año { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public int UsuarioId { get; set; }
        public int PerfilId { get; set; }
        
        // Navigation properties
        public virtual User? Usuario { get; set; }
    }
}
