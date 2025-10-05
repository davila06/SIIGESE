using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ICobroRepository : IRepository<Cobro>
    {
        Task<IEnumerable<Cobro>> GetCobrosByPolizaIdAsync(int polizaId);
        Task<IEnumerable<Cobro>> GetCobrosByEstadoAsync(EstadoCobro estado);
        Task<IEnumerable<Cobro>> GetCobrosVencidosAsync();
        Task<IEnumerable<Cobro>> GetCobrosByFechaRangoAsync(DateTime fechaDesde, DateTime fechaHasta);
        Task<Cobro?> GetByNumeroReciboAsync(string numeroRecibo);
        Task<bool> ExisteNumeroReciboAsync(string numeroRecibo);
        Task<decimal> GetTotalCobradoAsync(DateTime? fechaDesde = null, DateTime? fechaHasta = null);
        Task<int> GetTotalCobrosPendientesAsync();
        Task<decimal> GetMontoTotalPendienteAsync();
        Task<IEnumerable<Cobro>> GetCobrosProximosVencerAsync(int dias = 7);
    }
}