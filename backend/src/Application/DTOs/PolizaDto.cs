using System;

namespace Application.DTOs
{
    public class PolizaDto
    {
        public int Id { get; set; }
        public string NumeroPoliza { get; set; } = string.Empty;
        public string Modalidad { get; set; } = string.Empty;
        public string NombreAsegurado { get; set; } = string.Empty;
        public string NumeroCedula { get; set; } = string.Empty;
        public decimal Prima { get; set; }
        public string Moneda { get; set; } = string.Empty;
        public DateTime FechaVigencia { get; set; }
        public string Frecuencia { get; set; } = string.Empty;
        public string Aseguradora { get; set; } = string.Empty;
        public string Placa { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Año { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string NumeroTelefono { get; set; } = string.Empty;
        public int PerfilId { get; set; }
        public bool EsActivo { get; set; } = true;
        public string CreatedBy { get; set; } = string.Empty;
        public string? UpdatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}