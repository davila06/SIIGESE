using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data.Repositories;

namespace Infrastructure.Data.Repositories;

public class CotizacionRepository : Repository<Cotizacion>, ICotizacionRepository
{
    public CotizacionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Cotizacion>> GetByClienteAsync(string cliente)
    {
        return await _context.Cotizaciones
            .Where(c => c.NombreSolicitante.Contains(cliente))
            .OrderByDescending(c => c.FechaCotizacion)
            .ToListAsync();
    }

    public async Task<IEnumerable<Cotizacion>> GetByFechaRangeAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        return await _context.Cotizaciones
            .Where(c => c.FechaCotizacion >= fechaInicio && c.FechaCotizacion <= fechaFin)
            .OrderByDescending(c => c.FechaCotizacion)
            .ToListAsync();
    }

    public async Task<Cotizacion?> GetByNumeroCotizacionAsync(string numeroCotizacion)
    {
        return await _context.Cotizaciones
            .FirstOrDefaultAsync(c => c.NumeroCotizacion == numeroCotizacion);
    }

    public async Task<bool> ExistsNumeroCotizacionAsync(string numeroCotizacion)
    {
        return await _context.Cotizaciones
            .AnyAsync(c => c.NumeroCotizacion == numeroCotizacion);
    }
}