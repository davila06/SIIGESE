using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Interfaces
{
    /// <summary>
    /// Read / write access to the ReclamoHistoriales table.
    /// </summary>
    public interface IReclamoHistorialRepository : IRepository<ReclamoHistorial>
    {
        /// <summary>
        /// Returns all non-deleted history entries for the given reclamo,
        /// ordered newest-first.
        /// </summary>
        Task<IEnumerable<ReclamoHistorial>> GetByReclamoIdAsync(int reclamoId);
    }
}
