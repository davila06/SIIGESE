using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IPolizaRepository : IRepository<Poliza>
    {
        Task<Poliza?> GetByNumeroPolizaAsync(string numeroPoliza);
        Task<IEnumerable<Poliza>> GetByAseguradoraAsync(string aseguradora);
        Task<IEnumerable<Poliza>> GetByPerfilIdAsync(int perfilId);
        Task<IEnumerable<Poliza>> GetActivasAsync();
        Task<IEnumerable<Poliza>> GetByPlacaAsync(string placa);
        Task<IEnumerable<Poliza>> GetByFechaVigenciaAsync(DateTime fechaInicio, DateTime fechaFin);
    }
}