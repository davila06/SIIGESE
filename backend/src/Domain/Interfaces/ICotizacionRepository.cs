using Domain.Entities;

namespace Domain.Interfaces;

public interface ICotizacionRepository : IRepository<Cotizacion>
{
    Task<IEnumerable<Cotizacion>> GetByClienteAsync(string cliente);
    Task<IEnumerable<Cotizacion>> GetByFechaRangeAsync(DateTime fechaInicio, DateTime fechaFin);
    Task<Cotizacion?> GetByNumeroCotizacionAsync(string numeroCotizacion);
    Task<bool> ExistsNumeroCotizacionAsync(string numeroCotizacion);
}