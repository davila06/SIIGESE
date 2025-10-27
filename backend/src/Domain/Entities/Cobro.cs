using System;

namespace Domain.Entities
{
    public class Cobro : Entity
    {
        public string NumeroRecibo { get; set; } = string.Empty;
        public decimal MontoTotal { get; set; }
        public decimal MontoCobrado { get; set; }
        public DateTime FechaCobro { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public EstadoCobro Estado { get; set; } = EstadoCobro.Pendiente;
        public MetodoPago MetodoPago { get; set; } = MetodoPago.Efectivo;
        public string Moneda { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;
        public int PolizaId { get; set; }
        public string NumeroPoliza { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteApellido { get; set; } = string.Empty;
        public int UsuarioCobroId { get; set; }
        public string UsuarioCobroNombre { get; set; } = string.Empty;
        public bool IsDeleted { get; set; } = false;
        public string CreatedBy { get; set; } = string.Empty;
        public string? UpdatedBy { get; set; }
    }
}
