using System;

namespace Application.DTOs
{
    public class ReclamoDto
    {
        public int Id { get; set; }
        public string NumeroReclamo { get; set; } = string.Empty;
        public string NumeroPoliza { get; set; } = string.Empty;
        public string TipoReclamo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime FechaReclamo { get; set; }
        public DateTime? FechaLimiteRespuesta { get; set; }
        public DateTime? FechaResolucion { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string Prioridad { get; set; } = string.Empty;
        public decimal MontoReclamado { get; set; }
        public decimal? MontoAprobado { get; set; }
        public string NombreAsegurado { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteApellido { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;
        public string DocumentosAdjuntos { get; set; } = string.Empty;
        public int? UsuarioAsignadoId { get; set; }
        public string Moneda { get; set; } = "CRC";
        public string CreatedBy { get; set; } = string.Empty;
        public string? UpdatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
