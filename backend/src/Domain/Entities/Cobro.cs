using System;

namespace Domain.Entities
{
    public class Cobro : BaseEntity
    {
        public string NumeroRecibo { get; set; } = string.Empty;
        public int PolizaId { get; set; }
        public string NumeroPoliza { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteApellido { get; set; } = string.Empty;
        public DateTime FechaVencimiento { get; set; }
        public DateTime? FechaCobro { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal? MontoCobrado { get; set; }
        public EstadoCobro Estado { get; set; } = EstadoCobro.Pendiente;
        public MetodoPago? MetodoPago { get; set; }
        public string? Observaciones { get; set; }
        public int? UsuarioCobroId { get; set; }
        public string? UsuarioCobroNombre { get; set; }
        
        // Navigation properties
        public virtual Poliza Poliza { get; set; } = null!;
        public virtual User? UsuarioCobro { get; set; }
    }

    public enum EstadoCobro
    {
        Pendiente = 0,
        Cobrado = 1,
        Vencido = 2,
        Cancelado = 3
    }

    public enum MetodoPago
    {
        Efectivo = 0,
        Transferencia = 1,
        Cheque = 2,
        TarjetaCredito = 3,
        TarjetaDebito = 4
    }
}