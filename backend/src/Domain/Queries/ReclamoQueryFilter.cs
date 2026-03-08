namespace Domain.Queries
{
    /// <summary>
    /// Domain-level filter parameters for Reclamo queries.
    /// Kept in Domain so IReclamoRepository can reference it without creating
    /// a circular dependency on the Application layer.
    /// </summary>
    public class ReclamoQueryFilter
    {
        public string? NumeroPoliza { get; set; }
        public string? NumeroReclamo { get; set; }
        public Domain.Entities.EstadoReclamo? Estado { get; set; }
        public Domain.Entities.TipoReclamo? TipoReclamo { get; set; }
        public Domain.Entities.PrioridadReclamo? Prioridad { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public int? UsuarioAsignadoId { get; set; }
        public string? ClienteNombreCompleto { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
