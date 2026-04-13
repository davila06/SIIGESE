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

    public class SolicitarCambioEstadoCobroDto
    {
        public EstadoCobro EstadoSolicitado { get; set; }
        public string? Motivo { get; set; }
    }

    public class ResolverCambioEstadoCobroDto
    {
        public string? Motivo { get; set; }
    }

    public class CobroEstadoChangeRequestDto
    {
        public int Id { get; set; }
        public int CobroId { get; set; }
        public string NumeroRecibo { get; set; } = string.Empty;
        public string NumeroPoliza { get; set; } = string.Empty;
        public string ClienteNombreCompleto { get; set; } = string.Empty;
        public EstadoCobro EstadoActual { get; set; }
        public EstadoCobro EstadoSolicitado { get; set; }
        public EstadoSolicitudCambioCobro EstadoSolicitud { get; set; }
        public string? MotivoSolicitud { get; set; }
        public string? MotivoDecision { get; set; }
        public int SolicitadoPorUserId { get; set; }
        public string SolicitadoPorNombre { get; set; } = string.Empty;
        public string SolicitadoPorEmail { get; set; } = string.Empty;
        public int? ResueltoPorUserId { get; set; }
        public string? ResueltoPorNombre { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ResueltoAt { get; set; }
    }

    public class CobroChangeRequestActionResultDto
    {
        public CobroEstadoChangeRequestDto Request { get; set; } = new();
        public CobroDto Cobro { get; set; } = new();
    }
}