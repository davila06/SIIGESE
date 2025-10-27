namespace Domain.Entities
{
    public enum EstadoReclamo
    {
        Pendiente,
        Abierto,
        EnRevision,
        EnProceso,
        Aprobado,
        Rechazado,
        Resuelto,
        Cerrado
    }

    public enum TipoReclamo
    {
        Siniestro,
        Queja,
        Sugerencia,
        Reclamo,
        Otros
    }

    public enum PrioridadReclamo
    {
        Baja,
        Media,
        Alta,
        Critica
    }

    public enum EstadoCobro
    {
        Pendiente,
        Pagado,
        Cobrado,
        Vencido,
        Cancelado
    }

    public enum MetodoPago
    {
        Efectivo,
        Tarjeta,
        Transferencia,
        Cheque,
        Otros
    }
}