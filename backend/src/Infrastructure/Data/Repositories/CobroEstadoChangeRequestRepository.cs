using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories
{
    public class CobroEstadoChangeRequestRepository : ICobroEstadoChangeRequestRepository
    {
        private readonly ApplicationDbContext _context;

        public CobroEstadoChangeRequestRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CobroEstadoChangeRequest?> GetByIdAsync(int id)
        {
            return await _context.Set<CobroEstadoChangeRequest>()
                .Include(x => x.Cobro)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task<IEnumerable<CobroEstadoChangeRequest>> GetPendientesAsync()
        {
            return await _context.Set<CobroEstadoChangeRequest>()
                .Include(x => x.Cobro)
                .Where(x => !x.IsDeleted && x.EstadoSolicitud == EstadoSolicitudCambioCobro.Pendiente)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<CobroEstadoChangeRequest>> GetBySolicitanteAsync(int userId)
        {
            return await _context.Set<CobroEstadoChangeRequest>()
                .Include(x => x.Cobro)
                .Where(x => !x.IsDeleted && x.SolicitadoPorUserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> ExistePendienteParaCobroAsync(int cobroId)
        {
            return await _context.Set<CobroEstadoChangeRequest>()
                .AnyAsync(x => !x.IsDeleted
                    && x.CobroId == cobroId
                    && x.EstadoSolicitud == EstadoSolicitudCambioCobro.Pendiente);
        }

        public async Task<CobroEstadoChangeRequest> AddAsync(CobroEstadoChangeRequest request)
        {
            await _context.Set<CobroEstadoChangeRequest>().AddAsync(request);
            return request;
        }

        public Task UpdateAsync(CobroEstadoChangeRequest request)
        {
            request.UpdatedAt = System.DateTime.UtcNow;
            _context.Set<CobroEstadoChangeRequest>().Update(request);
            return Task.CompletedTask;
        }
    }
}
