using System;

namespace Application.DTOs
{
    public class ReclamoDto
    {
        public int Id { get; set; }
        public string NumeroReclamo { get; set; } = string.Empty;
        public int PolizaId { get; set; }
        public string NumeroPoliza { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteApellido { get; set; } = string.Empty;
        public DateTime FechaReclamo { get; set; }
        public DateTime? FechaResolucion { get; set; }
        public string TipoReclamo { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal? MontoReclamado { get; set; }
        public decimal? MontoAprobado { get; set; }
        public string Moneda { get; set; } = "CRC";
        public string Prioridad { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
        public string? DocumentosAdjuntos { get; set; }
        public int? UsuarioAsignadoId { get; set; }
        public string? UsuarioAsignadoNombre { get; set; }
        public DateTime? FechaLimiteRespuesta { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateReclamoDto
    {
        public int PolizaId { get; set; }
        public string NumeroPoliza { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteApellido { get; set; } = string.Empty;
        public int TipoReclamo { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public decimal? MontoReclamado { get; set; }
        public string Moneda { get; set; } = "CRC";
        public int Prioridad { get; set; } = 1; // Media por defecto
        public string? Observaciones { get; set; }
        public string? DocumentosAdjuntos { get; set; }
        public int? UsuarioAsignadoId { get; set; }
        public DateTime? FechaLimiteRespuesta { get; set; }
    }

    public class UpdateReclamoDto
    {
        public int Estado { get; set; }
        public string? Descripcion { get; set; }
        public decimal? MontoAprobado { get; set; }
        public int? Prioridad { get; set; }
        public string? Observaciones { get; set; }
        public string? DocumentosAdjuntos { get; set; }
        public int? UsuarioAsignadoId { get; set; }
        public DateTime? FechaLimiteRespuesta { get; set; }
    }

    public class ReclamoStatsDto
    {
        public int TotalReclamos { get; set; }
        public int ReclamosAbiertos { get; set; }
        public int ReclamosEnProceso { get; set; }
        public int ReclamosResueltos { get; set; }
        public int ReclamosCerrados { get; set; }
        public int ReclamosRechazados { get; set; }
        public decimal? TotalMontoReclamado { get; set; }
        public decimal? TotalMontoAprobado { get; set; }
        public string MonedaPrincipal { get; set; } = "CRC";
        public int ReclamosPrioridadAlta { get; set; }
        public int ReclamosPrioridadCritica { get; set; }
        public int ReclamosVencidos { get; set; }
    }

    public class ReclamoFilterDto
    {
        public int? Estado { get; set; }
        public int? TipoReclamo { get; set; }
        public int? Prioridad { get; set; }
        public string? ClienteNombre { get; set; }
        public string? NumeroPoliza { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public int? UsuarioAsignadoId { get; set; }
        public bool? SoloVencidos { get; set; }
        public string? Moneda { get; set; }
    }
}