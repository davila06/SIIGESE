using System;

namespace Application.DTOs
{
    // ─────────────────────────────────────────────────────────────────────────────
    // Historial DTOs
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>One entry in the reclamo audit timeline returned to the frontend.</summary>
    public class ReclamoHistorialEntryDto
    {
        public int Id { get; set; }
        public int ReclamoId { get; set; }

        /// <summary>
        /// "Creacion" | "CambioEstado" | "Asignacion" | "Resolucion" |
        /// "Actualizacion" | "DocumentoAgregado" | "DocumentoEliminado"
        /// </summary>
        public string TipoEvento { get; set; } = string.Empty;
        public string? ValorAnterior { get; set; }
        public string? ValorNuevo { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;

        /// <summary>When the event was recorded (maps to Entity.CreatedAt).</summary>
        public DateTime FechaEvento { get; set; }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Documentos DTOs
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Public-facing document metadata DTO — does NOT expose the physical storage path.
    /// </summary>
    public class ReclamoDocumentoDto
    {
        /// <summary>Stable GUID string assigned at upload time.</summary>
        public string Id { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public long Tamano { get; set; }
        public string TipoContenido { get; set; } = string.Empty;
        public DateTime FechaSubida { get; set; }
        public string SubidoPor { get; set; } = string.Empty;
    }

    /// <summary>
    /// Internal metadata stored in the Reclamo.DocumentosAdjuntos JSON column.
    /// Includes the physical storage path — never serialised to API responses.
    /// </summary>
    public class ReclamoDocumentoMetadata : ReclamoDocumentoDto
    {
        /// <summary>
        /// Path relative to the content-root uploads directory, e.g.
        /// "reclamos/42/b3f1a2_contrato.pdf".
        /// Swap the storage layer (local → Azure Blob) by changing only the
        /// controller helpers that read/write this field.
        /// </summary>
        public string Ruta { get; set; } = string.Empty;
    }
}
