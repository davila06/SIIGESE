using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IRoleRepository Roles { get; }
        IClienteRepository Clientes { get; }
        IDataRecordRepository DataRecords { get; }
        IPolizaRepository Polizas { get; }
        IPasswordResetTokenRepository PasswordResetTokens { get; }
        ICobroRepository Cobros { get; }
        IReclamoRepository Reclamos { get; }
        ICotizacionRepository Cotizaciones { get; }
        IEmailLogRepository EmailLogs { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}