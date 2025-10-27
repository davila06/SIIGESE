using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ICobroRepository
    {
        Task<IEnumerable<Cobro>> GetAllAsync();
        Task<Cobro?> GetByIdAsync(int id);
        Task<Cobro?> GetByNumeroReciboAsync(string numeroRecibo);
        Task<IEnumerable<Cobro>> GetCobrosByPolizaIdAsync(int polizaId);
        Task<IEnumerable<Cobro>> GetCobrosByEstadoAsync(EstadoCobro estado);
        Task<IEnumerable<Cobro>> GetCobrosVencidosAsync();
        Task<IEnumerable<Cobro>> GetCobrosProximosVencerAsync(int dias);
        Task<int> GetTotalCobrosPendientesAsync();
        Task<decimal> GetMontoTotalPendienteAsync();
        Task<decimal> GetTotalCobradoAsync();
        Task<bool> ExisteNumeroReciboAsync(string numeroRecibo);
        Task<Cobro> AddAsync(Cobro cobro);
        Task UpdateAsync(Cobro cobro);
        Task DeleteAsync(int id);
    }
}
