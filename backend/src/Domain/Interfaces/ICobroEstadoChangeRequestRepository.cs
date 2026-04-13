using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ICobroEstadoChangeRequestRepository
    {
        Task<CobroEstadoChangeRequest?> GetByIdAsync(int id);
        Task<IEnumerable<CobroEstadoChangeRequest>> GetPendientesAsync();
        Task<IEnumerable<CobroEstadoChangeRequest>> GetBySolicitanteAsync(int userId);
        Task<bool> ExistePendienteParaCobroAsync(int cobroId);
        Task<CobroEstadoChangeRequest> AddAsync(CobroEstadoChangeRequest request);
        Task UpdateAsync(CobroEstadoChangeRequest request);
    }
}
