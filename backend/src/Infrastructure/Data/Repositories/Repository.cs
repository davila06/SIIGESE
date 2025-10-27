using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Domain.Interfaces;
using Domain.Entities;

namespace Infrastructure.Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : Entity
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
            return entities;
        }

        public virtual async Task UpdateAsync(T entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(entity);
            await Task.CompletedTask;
        }

        public virtual async Task DeleteAsync(T entity)
        {
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(entity);
            await Task.CompletedTask;
        }

        public virtual async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                await DeleteAsync(entity);
            }
        }

        public virtual async Task<int> CountAsync()
        {
            return await _dbSet.CountAsync();
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.CountAsync(predicate);
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }
    }

    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context) { }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByUserNameAsync(string userName)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.UserName == userName);
        }

        public async Task<IEnumerable<User>> GetUsersWithRolesAsync()
        {
            return await _dbSet
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ToListAsync();
        }

        public async Task<User?> GetUserWithRolesAsync(int userId)
        {
            return await _dbSet
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task AssignRoleToUserAsync(int userId, int roleId)
        {
            var userRole = new UserRole
            {
                UserId = userId,
                RoleId = roleId
            };
            
            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();
        }

        public async Task ClearUserRolesAsync(int userId)
        {
            var userRoles = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .ToListAsync();
            
            _context.UserRoles.RemoveRange(userRoles);
            await _context.SaveChangesAsync();
        }

        public override async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _dbSet
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ToListAsync();
        }

        public override async Task<User?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
        }
    }

    public class RoleRepository : Repository<Role>, IRoleRepository
    {
        public RoleRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Role?> GetByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(r => r.Name == name);
        }

        public async Task<IEnumerable<Role>> GetActiveRolesAsync()
        {
            return await _dbSet.Where(r => r.IsActive).ToListAsync();
        }
    }

    public class ClienteRepository : Repository<Cliente>, IClienteRepository
    {
        public ClienteRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Cliente?> GetByCodigoAsync(string codigo)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.Codigo == codigo);
        }

        public async Task<Cliente?> GetByNITAsync(string nit)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.NIT == nit);
        }

        public async Task<IEnumerable<Cliente>> GetByPerfilIdAsync(int perfilId)
        {
            return await _dbSet.Where(c => c.PerfilId == perfilId).ToListAsync();
        }

        public async Task<IEnumerable<Cliente>> GetActivosAsync()
        {
            return await _dbSet.Where(c => c.EsActivo).ToListAsync();
        }
    }

    public class DataRecordRepository : Repository<DataRecord>, IDataRecordRepository
    {
        public DataRecordRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<DataRecord>> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(dr => dr.UploadedBy)
                .Where(dr => dr.UploadedByUserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<DataRecord>> GetByStatusAsync(string status)
        {
            return await _dbSet.Where(dr => dr.Status == status).ToListAsync();
        }

        public async Task<IEnumerable<DataRecord>> GetByPerfilIdAsync(int perfilId)
        {
            return await _dbSet.Where(dr => dr.PerfilId == perfilId).ToListAsync();
        }
    }

    public class PolizaRepository : Repository<Poliza>, IPolizaRepository
    {
        public PolizaRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Poliza?> GetByNumeroPolizaAsync(string numeroPoliza)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.NumeroPoliza == numeroPoliza);
        }

        public async Task<IEnumerable<Poliza>> GetByAseguradoraAsync(string aseguradora)
        {
            return await _dbSet.Where(p => p.Aseguradora == aseguradora).ToListAsync();
        }

        public async Task<IEnumerable<Poliza>> GetByPerfilIdAsync(int perfilId)
        {
            return await _dbSet.Where(p => p.PerfilId == perfilId).ToListAsync();
        }

        public async Task<IEnumerable<Poliza>> GetActivasAsync()
        {
            return await _dbSet.Where(p => p.EsActivo).ToListAsync();
        }

        public async Task<IEnumerable<Poliza>> GetByPlacaAsync(string placa)
        {
            return await _dbSet.Where(p => p.Placa == placa).ToListAsync();
        }

        public async Task<IEnumerable<Poliza>> GetByFechaVigenciaAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            return await _dbSet.Where(p => p.FechaVigencia >= fechaInicio && p.FechaVigencia <= fechaFin).ToListAsync();
        }
    }
}