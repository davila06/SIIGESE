using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Domain.Interfaces;

namespace Infrastructure.Data.Repositories
{
    public class CobroRepository : Repository<Cobro>, ICobroRepository
    {
        public CobroRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Cobro>> GetCobrosByPolizaIdAsync(int polizaId)
        {
            return await _context.Cobros
                .Include(c => c.Poliza)
                .Include(c => c.UsuarioCobro)
                .Where(c => c.PolizaId == polizaId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Cobro>> GetCobrosByEstadoAsync(EstadoCobro estado)
        {
            return await _context.Cobros
                .Include(c => c.Poliza)
                .Include(c => c.UsuarioCobro)
                .Where(c => c.Estado == estado)
                .OrderBy(c => c.FechaVencimiento)
                .ToListAsync();
        }

        public async Task<IEnumerable<Cobro>> GetCobrosVencidosAsync()
        {
            var fechaActual = DateTime.UtcNow.Date;
            return await _context.Cobros
                .Include(c => c.Poliza)
                .Include(c => c.UsuarioCobro)
                .Where(c => c.Estado == EstadoCobro.Pendiente && c.FechaVencimiento.Date < fechaActual)
                .OrderBy(c => c.FechaVencimiento)
                .ToListAsync();
        }

        public async Task<IEnumerable<Cobro>> GetCobrosByFechaRangoAsync(DateTime fechaDesde, DateTime fechaHasta)
        {
            return await _context.Cobros
                .Include(c => c.Poliza)
                .Include(c => c.UsuarioCobro)
                .Where(c => c.FechaCobro.HasValue && 
                           c.FechaCobro.Value.Date >= fechaDesde.Date && 
                           c.FechaCobro.Value.Date <= fechaHasta.Date)
                .OrderByDescending(c => c.FechaCobro)
                .ToListAsync();
        }

        public async Task<Cobro?> GetByNumeroReciboAsync(string numeroRecibo)
        {
            return await _context.Cobros
                .Include(c => c.Poliza)
                .Include(c => c.UsuarioCobro)
                .FirstOrDefaultAsync(c => c.NumeroRecibo == numeroRecibo);
        }

        public async Task<bool> ExisteNumeroReciboAsync(string numeroRecibo)
        {
            return await _context.Cobros
                .AnyAsync(c => c.NumeroRecibo == numeroRecibo);
        }

        public async Task<decimal> GetTotalCobradoAsync(DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            var query = _context.Cobros
                .Where(c => c.Estado == EstadoCobro.Cobrado && c.MontoCobrado.HasValue);

            if (fechaDesde.HasValue)
            {
                query = query.Where(c => c.FechaCobro.HasValue && c.FechaCobro.Value.Date >= fechaDesde.Value.Date);
            }

            if (fechaHasta.HasValue)
            {
                query = query.Where(c => c.FechaCobro.HasValue && c.FechaCobro.Value.Date <= fechaHasta.Value.Date);
            }

            return await query.SumAsync(c => c.MontoCobrado.Value);
        }

        public async Task<int> GetTotalCobrosPendientesAsync()
        {
            return await _context.Cobros
                .CountAsync(c => c.Estado == EstadoCobro.Pendiente);
        }

        public async Task<decimal> GetMontoTotalPendienteAsync()
        {
            return await _context.Cobros
                .Where(c => c.Estado == EstadoCobro.Pendiente)
                .SumAsync(c => c.MontoTotal);
        }

        public async Task<IEnumerable<Cobro>> GetCobrosProximosVencerAsync(int dias = 7)
        {
            var fechaLimite = DateTime.UtcNow.Date.AddDays(dias);
            return await _context.Cobros
                .Include(c => c.Poliza)
                .Include(c => c.UsuarioCobro)
                .Where(c => c.Estado == EstadoCobro.Pendiente && 
                           c.FechaVencimiento.Date <= fechaLimite && 
                           c.FechaVencimiento.Date >= DateTime.UtcNow.Date)
                .OrderBy(c => c.FechaVencimiento)
                .ToListAsync();
        }
    }
}