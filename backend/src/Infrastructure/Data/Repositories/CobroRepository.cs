using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;

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
                .Where(c => c.PolizaId == polizaId && !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Cobro>> GetCobrosByEstadoAsync(EstadoCobro estado)
        {
            return await _context.Cobros
                .Where(c => c.Estado == estado && !c.IsDeleted)
                .OrderBy(c => c.FechaVencimiento)
                .ToListAsync();
        }

        public async Task<IEnumerable<Cobro>> GetCobrosVencidosAsync()
        {
            var fechaActual = DateTime.UtcNow.Date;
            return await _context.Cobros
                .Where(c => c.Estado == EstadoCobro.Pendiente && 
                           c.FechaVencimiento.Date < fechaActual && 
                           !c.IsDeleted)
                .OrderBy(c => c.FechaVencimiento)
                .ToListAsync();
        }

        public async Task<IEnumerable<Cobro>> GetCobrosProximosVencerAsync(int dias)
        {
            var fechaLimite = DateTime.UtcNow.AddDays(dias);
            return await _context.Cobros
                .Where(c => c.FechaVencimiento <= fechaLimite && 
                           c.FechaVencimiento > DateTime.UtcNow &&
                           c.Estado != EstadoCobro.Pagado && 
                           !c.IsDeleted)
                .OrderBy(c => c.FechaVencimiento)
                .ToListAsync();
        }

        public async Task<IEnumerable<Cobro>> GetCobrosProximosPorPeriodicidadAsync()
        {
            // Show 13 months ahead so that ANUAL cobros are always included;
            // for other frequencies a 30-day window is sufficient.
            var fechaLimiteCorto  = DateTime.UtcNow.AddDays(30);
            var fechaLimiteLargo  = DateTime.UtcNow.AddMonths(13);

            return await (
                from c in _context.Cobros
                join p in _context.Polizas on c.PolizaId equals p.Id
                where !c.IsDeleted
                      && !p.IsDeleted
                      && c.Estado == EstadoCobro.Pendiente
                      && (
                          // MENSUAL: always show
                          (p.Frecuencia ?? string.Empty).ToUpper() == "MENSUAL"
                          ||
                          // ANUAL/SEMESTRAL/etc.: show within their full cycle window
                          (
                              (p.Frecuencia ?? string.Empty).ToUpper() == "ANUAL" ||
                              (p.Frecuencia ?? string.Empty).ToUpper() == "ANNUAL" ||
                              (p.Frecuencia ?? string.Empty).ToUpper() == "YEARLY"
                          ) && c.FechaVencimiento <= fechaLimiteLargo
                          ||
                          // Everything else: next 30 days
                          (
                              (p.Frecuencia ?? string.Empty).ToUpper() != "MENSUAL" &&
                              (p.Frecuencia ?? string.Empty).ToUpper() != "ANUAL" &&
                              (p.Frecuencia ?? string.Empty).ToUpper() != "ANNUAL" &&
                              (p.Frecuencia ?? string.Empty).ToUpper() != "YEARLY"
                          ) && c.FechaVencimiento <= fechaLimiteCorto
                      )
                orderby c.FechaVencimiento
                select c
            ).ToListAsync();
        }

        public async Task<IEnumerable<Cobro>> GetCobrosByFrecuenciaAsync(string frecuencia)
        {
            var frecuenciaUpper = frecuencia.ToUpper().Trim();
            return await (
                from c in _context.Cobros
                join p in _context.Polizas on c.PolizaId equals p.Id
                where !c.IsDeleted
                      && !p.IsDeleted
                      && (p.Frecuencia ?? string.Empty).ToUpper() == frecuenciaUpper
                orderby c.FechaVencimiento
                select c
            ).ToListAsync();
        }

        public async Task<Cobro?> GetByNumeroReciboAsync(string numeroRecibo)
        {
            return await _context.Cobros
                .FirstOrDefaultAsync(c => c.NumeroRecibo == numeroRecibo && !c.IsDeleted);
        }

        public async Task<int> GetTotalCobrosPendientesAsync()
        {
            return await _context.Cobros
                .CountAsync(c => c.Estado == EstadoCobro.Pendiente && !c.IsDeleted);
        }

        public async Task<decimal> GetMontoTotalPendienteAsync()
        {
            return await _context.Cobros
                .Where(c => c.Estado == EstadoCobro.Pendiente && !c.IsDeleted)
                .SumAsync(c => c.MontoTotal);
        }

        public async Task<decimal> GetTotalCobradoAsync()
        {
            return await _context.Cobros
                .Where(c => c.Estado == EstadoCobro.Pagado && !c.IsDeleted)
                .SumAsync(c => c.MontoCobrado);
        }

        public async Task<bool> ExisteNumeroReciboAsync(string numeroRecibo)
        {
            return await _context.Cobros
                .AnyAsync(c => c.NumeroRecibo == numeroRecibo && !c.IsDeleted);
        }

        public async Task<int> GetTotalCountAsync()
            => await _context.Cobros.CountAsync(c => !c.IsDeleted);

        public async Task<int> GetCobrosVencidosCountAsync()
        {
            var hoy = DateTime.UtcNow.Date;
            return await _context.Cobros.CountAsync(c =>
                c.Estado == EstadoCobro.Pendiente &&
                c.FechaVencimiento.Date < hoy &&
                !c.IsDeleted);
        }

        public async Task<int> GetCobrosProximosVencerCountAsync(int dias)
        {
            var fechaLimite = DateTime.UtcNow.AddDays(dias);
            return await _context.Cobros.CountAsync(c =>
                c.FechaVencimiento <= fechaLimite &&
                c.FechaVencimiento > DateTime.UtcNow &&
                c.Estado != EstadoCobro.Pagado &&
                !c.IsDeleted);
        }

        // Implementación explícita para evitar conflictos con Repository base
        async Task<Cobro> ICobroRepository.AddAsync(Cobro cobro)
        {
            return await base.AddAsync(cobro);
        }
    }
}