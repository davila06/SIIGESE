using System;

namespace Domain.Entities
{
    public class Poliza : Entity
    {
        public string? NumeroPoliza { get; set; }
        public string? Modalidad { get; set; }
        public string? NombreAsegurado { get; set; }
        public string? NumeroCedula { get; set; }
        public decimal Prima { get; set; }
        public string? Moneda { get; set; }
        public DateTime FechaVigencia { get; set; }
        public string? Frecuencia { get; set; }
        public string? Aseguradora { get; set; }
        public string? Placa { get; set; }
        public string? Marca { get; set; }
        public string? Modelo { get; set; }
        public string? Año { get; set; }
        public string? Correo { get; set; }
        public string? NumeroTelefono { get; set; }
        public int PerfilId { get; set; }
        public bool EsActivo { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public string CreatedBy { get; set; } = string.Empty;
        public string? UpdatedBy { get; set; }
    }
}
