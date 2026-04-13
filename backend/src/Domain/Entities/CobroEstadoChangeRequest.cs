using System;

namespace Domain.Entities
{
    public class CobroEstadoChangeRequest : Entity
    {
        public int CobroId { get; set; }
        public EstadoCobro EstadoActual { get; set; }
        public EstadoCobro EstadoSolicitado { get; set; }
        public EstadoSolicitudCambioCobro EstadoSolicitud { get; set; } = EstadoSolicitudCambioCobro.Pendiente;

        public string? MotivoSolicitud { get; set; }
        public string? MotivoDecision { get; set; }

        public int SolicitadoPorUserId { get; set; }
        public string SolicitadoPorNombre { get; set; } = string.Empty;
        public string SolicitadoPorEmail { get; set; } = string.Empty;

        public int? ResueltoPorUserId { get; set; }
        public string? ResueltoPorNombre { get; set; }
        public DateTime? ResueltoAt { get; set; }

        public virtual Cobro Cobro { get; set; } = null!;
    }
}
