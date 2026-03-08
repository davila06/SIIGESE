using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Queries;
using Infrastructure.Data;

namespace Infrastructure.Data.Repositories
{
    public class ReclamoRepository : Repository<Reclamo>, IReclamoRepository
    {
        public ReclamoRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Reclamo>> GetReclamosByPolizaIdAsync(string numeroPoliza)
        {
            return await _context.Set<Reclamo>()
                .Include(r => r.UsuarioAsignado)
                .Where(r => r.NumeroPoliza == numeroPoliza && !r.IsDeleted)
                .OrderByDescending(r => r.FechaReclamo)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reclamo>> GetReclamosByEstadoAsync(EstadoReclamo estado)
        {
            return await _context.Set<Reclamo>()
                .Include(r => r.Poliza)
                .Include(r => r.UsuarioAsignado)
                .Where(r => r.Estado == estado && !r.IsDeleted)
                .OrderByDescending(r => r.FechaReclamo)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reclamo>> GetReclamosByTipoAsync(TipoReclamo tipo)
        {
            return await _context.Set<Reclamo>()
                .Include(r => r.Poliza)
                .Include(r => r.UsuarioAsignado)
                .Where(r => r.TipoReclamo == tipo && !r.IsDeleted)
                .OrderByDescending(r => r.FechaReclamo)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reclamo>> GetReclamosByPrioridadAsync(PrioridadReclamo prioridad)
        {
            return await _context.Set<Reclamo>()
                .Include(r => r.Poliza)
                .Include(r => r.UsuarioAsignado)
                .Where(r => r.Prioridad == prioridad && !r.IsDeleted)
                .OrderByDescending(r => r.FechaReclamo)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reclamo>> GetReclamosByUsuarioAsignadoAsync(int usuarioId)
        {
            return await _context.Set<Reclamo>()
                .Include(r => r.Poliza)
                .Include(r => r.UsuarioAsignado)
                .Where(r => r.UsuarioAsignadoId == usuarioId && !r.IsDeleted)
                .OrderByDescending(r => r.FechaReclamo)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reclamo>> GetReclamosVencidosAsync()
        {
            var fechaActual = DateTime.UtcNow;
            return await _context.Set<Reclamo>()
                .Include(r => r.Poliza)
                .Include(r => r.UsuarioAsignado)
                .Where(r => r.FechaLimiteRespuesta.HasValue && 
                           r.FechaLimiteRespuesta.Value < fechaActual && 
                           r.Estado != EstadoReclamo.Resuelto &&
                           r.Estado != EstadoReclamo.Cerrado &&
                           !r.IsDeleted)
                .OrderBy(r => r.FechaLimiteRespuesta)
                .ToListAsync();
        }

        public async Task<string> GenerateNumeroReclamoAsync()
        {
            var fechaActual = DateTime.UtcNow;
            var year = fechaActual.Year;
            var month = fechaActual.Month;

            var ultimoReclamo = await _context.Set<Reclamo>()
                .Where(r => r.NumeroReclamo.StartsWith($"REC-{year}-{month:D2}"))
                .OrderByDescending(r => r.NumeroReclamo)
                .FirstOrDefaultAsync();

            int siguienteNumero = 1;
            if (ultimoReclamo != null)
            {
                var partes = ultimoReclamo.NumeroReclamo.Split('-');
                if (partes.Length == 4 && int.TryParse(partes[3], out int numero))
                {
                    siguienteNumero = numero + 1;
                }
            }

            return $"REC-{year}-{month:D2}-{siguienteNumero:D4}";
        }

        // -----------------------------------------------------------------------
        // SQL-level filter with server-side pagination
        // -----------------------------------------------------------------------
        public async Task<(IEnumerable<Reclamo> Items, int TotalCount)> GetByFiltroAsync(ReclamoQueryFilter filtro)
        {
            var query = _context.Set<Reclamo>()
                .Include(r => r.UsuarioAsignado)
                .Where(r => !r.IsDeleted);

            if (!string.IsNullOrWhiteSpace(filtro.NumeroPoliza))
                query = query.Where(r => r.NumeroPoliza.Contains(filtro.NumeroPoliza));

            if (!string.IsNullOrWhiteSpace(filtro.NumeroReclamo))
                query = query.Where(r => r.NumeroReclamo.Contains(filtro.NumeroReclamo));

            if (filtro.Estado.HasValue)
                query = query.Where(r => r.Estado == filtro.Estado.Value);

            if (filtro.TipoReclamo.HasValue)
                query = query.Where(r => r.TipoReclamo == filtro.TipoReclamo.Value);

            if (filtro.Prioridad.HasValue)
                query = query.Where(r => r.Prioridad == filtro.Prioridad.Value);

            if (filtro.FechaDesde.HasValue)
                query = query.Where(r => r.FechaReclamo >= filtro.FechaDesde.Value);

            if (filtro.FechaHasta.HasValue)
                query = query.Where(r => r.FechaReclamo <= filtro.FechaHasta.Value);

            if (filtro.UsuarioAsignadoId.HasValue)
                query = query.Where(r => r.UsuarioAsignadoId == filtro.UsuarioAsignadoId.Value);

            if (!string.IsNullOrWhiteSpace(filtro.ClienteNombreCompleto))
                query = query.Where(r => r.ClienteNombreCompleto.Contains(filtro.ClienteNombreCompleto));

            var totalCount = await query.CountAsync();

            var pageNumber = Math.Max(1, filtro.PageNumber);
            var pageSize   = Math.Clamp(filtro.PageSize, 1, 100);

            var items = await query
                .OrderByDescending(r => r.FechaReclamo)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        // -----------------------------------------------------------------------
        // Aggregate queries — one SELECT COUNT/SUM each, no full table scan
        // -----------------------------------------------------------------------
        public async Task<int> GetTotalCountAsync()
            => await _context.Set<Reclamo>().CountAsync(r => !r.IsDeleted);

        public async Task<int> GetCountByEstadoAsync(EstadoReclamo estado)
            => await _context.Set<Reclamo>().CountAsync(r => r.Estado == estado && !r.IsDeleted);

        public async Task<decimal> GetMontoTotalReclamadoAsync()
            => await _context.Set<Reclamo>()
                .Where(r => !r.IsDeleted)
                .SumAsync(r => r.MontoReclamado);

        public async Task<decimal> GetMontoTotalAprobadoAsync()
            => await _context.Set<Reclamo>()
                .Where(r => !r.IsDeleted && r.MontoAprobado.HasValue)
                .SumAsync(r => r.MontoAprobado ?? 0m);
    }
}