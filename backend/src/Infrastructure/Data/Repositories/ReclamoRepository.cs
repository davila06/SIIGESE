using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Application.DTOs;

namespace Infrastructure.Data.Repositories
{
    public class ReclamoRepository : Repository<Reclamo>, IReclamoRepository
    {
        public ReclamoRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Reclamo>> GetReclamosByPolizaIdAsync(int polizaId)
        {
            return await _context.Set<Reclamo>()
                .Include(r => r.Poliza)
                .Include(r => r.UsuarioAsignado)
                .Where(r => r.PolizaId == polizaId && !r.IsDeleted)
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
    }
}