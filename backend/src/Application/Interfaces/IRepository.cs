using Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
        Task<bool> ValidateTokenAsync(string token);
        Task LogoutAsync(string token);
    }

    public interface IClienteService
    {
        Task<IEnumerable<ClienteDto>> GetAllAsync();
        Task<IEnumerable<ClienteDto>> GetByPerfilIdAsync(int perfilId);
        Task<ClienteDto?> GetByIdAsync(int id);
        Task<ClienteDto> CreateAsync(CreateClienteDto dto);
        Task<ClienteDto> UpdateAsync(int id, CreateClienteDto dto);
        Task DeleteAsync(int id);
        Task<DataUploadResultDto> ProcesarExcelAsync(int perfilId, IFormFile file, int userId);
    }

    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<UserDto?> GetByEmailAsync(string email);
        Task<UserDto> CreateUserAsync(CreateUserDto dto);
        Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto dto);
        Task<bool> DeleteUserAsync(int id);
        Task<IEnumerable<RoleDto>> GetAllRolesAsync();
    }

    public interface IRoleService
    {
        Task<IEnumerable<RoleDto>> GetAllAsync();
        Task<RoleDto?> GetByIdAsync(int id);
        Task<RoleDto> CreateAsync(RoleDto dto);
        Task<RoleDto> UpdateAsync(int id, RoleDto dto);
        Task DeleteAsync(int id);
    }

    public interface IPolizaService
    {
        Task<IEnumerable<PolizaDto>> GetAllAsync();
        Task<IEnumerable<PolizaDto>> GetByPerfilIdAsync(int perfilId);
        Task<PolizaDto?> GetByIdAsync(int id);
        Task<PolizaDto?> GetByNumeroPolizaAsync(string numeroPoliza);
        Task<IEnumerable<PolizaDto>> GetByAseguradoraAsync(string aseguradora);
        Task<PolizaDto> CreateAsync(CreatePolizaDto dto);
        Task<PolizaDto> UpdateAsync(int id, CreatePolizaDto dto);
        Task DeleteAsync(int id);
        Task<DataUploadResultDto> ProcesarExcelPolizasAsync(int perfilId, IFormFile file, int userId);
    }
}