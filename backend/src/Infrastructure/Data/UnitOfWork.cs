using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Domain.Interfaces;
using Infrastructure.Data.Repositories;

namespace Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Users = new UserRepository(_context);
            Roles = new RoleRepository(_context);
            Clientes = new ClienteRepository(_context);
            DataRecords = new DataRecordRepository(_context);
            Polizas = new PolizaRepository(_context);
            PasswordResetTokens = new PasswordResetTokenRepository(_context);
            Cobros = new CobroRepository(_context);
            Reclamos = new ReclamoRepository(_context);
            Cotizaciones = new CotizacionRepository(_context);
        }

        public IUserRepository Users { get; }
        public IRoleRepository Roles { get; }
        public IClienteRepository Clientes { get; }
        public IDataRecordRepository DataRecords { get; }
        public IPolizaRepository Polizas { get; }
        public IPasswordResetTokenRepository PasswordResetTokens { get; }
        public ICobroRepository Cobros { get; }
        public IReclamoRepository Reclamos { get; }
        public ICotizacionRepository Cotizaciones { get; }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}