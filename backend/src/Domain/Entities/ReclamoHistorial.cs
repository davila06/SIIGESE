using System;

namespace Domain.Entities
{
    /// <summary>
    /// Immutable audit-trail entry for a reclamo.
    /// One row is written for every state change, assignment, resolution, or document event.
    /// </summary>
    public class ReclamoHistorial : Entity
    {
        /// <summary>FK to the parent Reclamo.</summary>
        public int ReclamoId { get; set; }

        /// <summary>
        /// Discriminator string — used by the UI to pick the right icon/colour.
        /// Values: "Creacion" | "CambioEstado" | "Asignacion" | "Resolucion" |
        ///         "Actualizacion" | "DocumentoAgregado" | "DocumentoEliminado"
        /// </summary>
        public string TipoEvento { get; set; } = string.Empty;

        /// <summary>Human-readable previous value (e.g. "Pendiente").</summary>
        public string? ValorAnterior { get; set; }

        /// <summary>Human-readable new value (e.g. "EnProceso").</summary>
        public string? ValorNuevo { get; set; }

        /// <summary>Free-text description composed by the service layer.</summary>
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>Username / email of whoever triggered the event.</summary>
        public string Usuario { get; set; } = "Sistema";

        // Navigation property
        public virtual Reclamo? Reclamo { get; set; }
    }
}
