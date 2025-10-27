using System;
using Domain.Entities;

namespace Application.DTOs
{
    public class CreateReclamoDto
    {
        public string NumeroPoliza { get; set; } = string.Empty;
        public TipoReclamo TipoReclamo { get; set; } = TipoReclamo.Reclamo;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime? FechaLimiteRespuesta { get; set; }
        public PrioridadReclamo Prioridad { get; set; } = PrioridadReclamo.Media;
        public decimal MontoReclamado { get; set; }
        public string NombreAsegurado { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteApellido { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;
        public int? UsuarioAsignadoId { get; set; }
        public string Moneda { get; set; } = "CRC";
    }

    public class UpdateReclamoDto
    {
        public int Id { get; set; }
        public TipoReclamo TipoReclamo { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public DateTime? FechaLimiteRespuesta { get; set; }
        public DateTime? FechaResolucion { get; set; }
        public EstadoReclamo Estado { get; set; }
        public PrioridadReclamo Prioridad { get; set; }
        public decimal MontoReclamado { get; set; }
        public decimal? MontoAprobado { get; set; }
        public string Observaciones { get; set; } = string.Empty;
        public string DocumentosAdjuntos { get; set; } = string.Empty;
        public int? UsuarioAsignadoId { get; set; }
    }

    public class ReclamoStatsDto
    {
        public int TotalReclamos { get; set; }
        public int ReclamosPendientes { get; set; }
        public int ReclamosEnProceso { get; set; }
        public int ReclamosResueltos { get; set; }
        public int ReclamosRechazados { get; set; }
        public decimal MontoTotalReclamado { get; set; }
        public decimal MontoTotalAprobado { get; set; }
        public decimal TasaAprobacion { get; set; }
    }

    public class ReclamoFilterDto
    {
        public string? NumeroPoliza { get; set; }
        public string? NumeroReclamo { get; set; }
        public EstadoReclamo? Estado { get; set; }
        public TipoReclamo? TipoReclamo { get; set; }
        public PrioridadReclamo? Prioridad { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public int? UsuarioAsignadoId { get; set; }
        public string? ClienteNombre { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}