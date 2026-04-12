using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;

namespace Infrastructure.Data.Repositories
{
    public class ReclamoHistorialRepository : Repository<ReclamoHistorial>, IReclamoHistorialRepository
    {
        public ReclamoHistorialRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ReclamoHistorial>> GetByReclamoIdAsync(int reclamoId)
        {
            return await _context.Set<ReclamoHistorial>()
                .Where(h => h.ReclamoId == reclamoId && !h.IsDeleted)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();
        }
    }
}
