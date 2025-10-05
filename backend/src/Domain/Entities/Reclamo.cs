using System;

namespace Domain.Entities
{
    public class Reclamo : BaseEntity
    {
        public string NumeroReclamo { get; set; } = string.Empty;
        public int PolizaId { get; set; }
        public string NumeroPoliza { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteApellido { get; set; } = string.Empty;
        public DateTime FechaReclamo { get; set; } = DateTime.UtcNow;
        public DateTime? FechaResolucion { get; set; }
        public TipoReclamo TipoReclamo { get; set; }
        public EstadoReclamo Estado { get; set; } = EstadoReclamo.Abierto;
        public string Descripcion { get; set; } = string.Empty;
        public decimal? MontoReclamado { get; set; }
        public decimal? MontoAprobado { get; set; }
        public string Moneda { get; set; } = "CRC"; // Código de moneda por defecto CRC (Colón Costarricense)
        public PrioridadReclamo Prioridad { get; set; } = PrioridadReclamo.Media;
        public string? Observaciones { get; set; }
        public string? DocumentosAdjuntos { get; set; } // JSON con rutas de documentos
        public int? UsuarioAsignadoId { get; set; }
        public string? UsuarioAsignadoNombre { get; set; }
        public DateTime? FechaLimiteRespuesta { get; set; }
        
        // Navigation properties
        public virtual Poliza Poliza { get; set; } = null!;
        public virtual User? UsuarioAsignado { get; set; }
    }

    public enum TipoReclamo
    {
        Siniestro = 0,
        Servicio = 1,
        Facturacion = 2,
        Cobertura = 3,
        Cancelacion = 4,
        Renovacion = 5,
        Otros = 6
    }

    public enum EstadoReclamo
    {
        Abierto = 0,
        EnProceso = 1,
        Resuelto = 2,
        Cerrado = 3,
        Rechazado = 4,
        Escalado = 5
    }

    public enum PrioridadReclamo
    {
        Baja = 0,
        Media = 1,
        Alta = 2,
        Critica = 3
    }
}