using System;

namespace Domain.Entities
{
    public class Reclamo : Entity
    {
        public string NumeroReclamo { get; set; } = string.Empty;
        public string NumeroPoliza { get; set; } = string.Empty;
        public TipoReclamo TipoReclamo { get; set; } = TipoReclamo.Reclamo;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime FechaReclamo { get; set; }
        public DateTime? FechaLimiteRespuesta { get; set; }
        public DateTime? FechaResolucion { get; set; }
        public EstadoReclamo Estado { get; set; } = EstadoReclamo.Pendiente;
        public PrioridadReclamo Prioridad { get; set; } = PrioridadReclamo.Media;
        public decimal MontoReclamado { get; set; }
        public decimal? MontoAprobado { get; set; }
        public string NombreAsegurado { get; set; } = string.Empty;
        public string ClienteNombreCompleto { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;
        public string DocumentosAdjuntos { get; set; } = string.Empty;
        public int? UsuarioAsignadoId { get; set; }
        public string Moneda { get; set; } = "CRC";
        
        // Navigation properties
        public virtual User? UsuarioAsignado { get; set; }
        public virtual Poliza? Poliza { get; set; }
    }
}
