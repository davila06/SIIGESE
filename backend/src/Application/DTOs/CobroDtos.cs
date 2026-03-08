using System;
using Domain.Entities;

namespace Application.DTOs
{
    public class CobroStatsDto
    {
        public int TotalCobros { get; set; }
        public int CobrosPendientes { get; set; }
        public int CobrosPagados { get; set; }
        public int CobrosVencidos { get; set; }
        public decimal MontoTotalPendiente { get; set; }
        public decimal MontoTotalCobrado { get; set; }
        public decimal PorcentajeCobrado { get; set; }
        public int CobrosProximosVencer { get; set; }
    }

    public class CobroRequestDto
    {
        public string NumeroRecibo { get; set; } = string.Empty;
        public decimal MontoTotal { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public MetodoPago MetodoPago { get; set; } = MetodoPago.Efectivo;
        public string Moneda { get; set; } = "CRC";
        public string Observaciones { get; set; } = string.Empty;
        public int PolizaId { get; set; }
        public string NumeroPoliza { get; set; } = string.Empty;
        public string ClienteNombreCompleto { get; set; } = string.Empty;
        public string? CorreoElectronico { get; set; }
        public int UsuarioCobroId { get; set; }
    }

    public class ActualizarCobroDto
    {
        public int Id { get; set; }
        public decimal? MontoCobrado { get; set; }
        public DateTime? FechaCobro { get; set; }
        public EstadoCobro Estado { get; set; }
        public MetodoPago MetodoPago { get; set; }
        public string Observaciones { get; set; } = string.Empty;
    }

    public class RegistrarCobroRequestDto
    {
        public int CobroId { get; set; }
        public decimal MontoCobrado { get; set; }
        public DateTime FechaCobro { get; set; }
        public MetodoPago MetodoPago { get; set; }
        public string Observaciones { get; set; } = string.Empty;
    }

    public class CancelarCobroDto
    {
        public string? Motivo { get; set; }
    }
}