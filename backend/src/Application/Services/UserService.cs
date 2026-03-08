using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using BCrypt.Net;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, IRoleRepository roleRepository, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            // Filtrar usuarios eliminados
            return users.Where(u => !u.IsDeleted).Select(MapToDto);
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            // No retornar usuarios eliminados
            return user != null && !user.IsDeleted ? MapToDto(user) : null;
        }

        public async Task<UserDto?> GetByEmailAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            // No retornar usuarios eliminados
            return user != null && !user.IsDeleted ? MapToDto(user) : null;
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto dto)
        {
            // Validar que el usuario no exista
            var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Ya existe un usuario con ese email");
            }

            var existingUserName = await _userRepository.GetByUserNameAsync(dto.UserName);
            if (existingUserName != null)
            {
                throw new InvalidOperationException("Ya existe un usuario con ese nombre de usuario");
            }

            // Crear entidad de usuario
            var user = new User
            {
                UserName = dto.UserName,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PasswordHash = HashPassword(dto.Password),
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            // Crear usuario
            var createdUser = await _userRepository.AddAsync(user);

            // Asignar roles
            if (dto.RoleIds.Any())
            {
                await AssignRolesToUser(createdUser.Id, dto.RoleIds);
            }

            // Recargar usuario con roles
            var userWithRoles = await _userRepository.GetByIdAsync(createdUser.Id);
            return MapToDto(userWithRoles!);
        }

        public async Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto dto)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return null;
            }

            // Validar email único (excluyendo el usuario actual)
            var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
            if (existingUser != null && existingUser.Id != id)
            {
                throw new InvalidOperationException("Ya existe un usuario con ese email");
            }

            // Validar username único (excluyendo el usuario actual)
            var existingUserName = await _userRepository.GetByUserNameAsync(dto.UserName);
            if (existingUserName != null && existingUserName.Id != id)
            {
                throw new InvalidOperationException("Ya existe un usuario con ese nombre de usuario");
            }

            // Actualizar propiedades
            user.UserName = dto.UserName;
            user.Email = dto.Email;
            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.IsActive = dto.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            // Actualizar usuario
            await _userRepository.UpdateAsync(user);

            // Actualizar roles
            await _userRepository.ClearUserRolesAsync(id);
            if (dto.RoleIds.Any())
            {
                await AssignRolesToUser(id, dto.RoleIds);
            }

            // Recargar usuario con roles
            var userWithRoles = await _userRepository.GetByIdAsync(id);
            return MapToDto(userWithRoles!);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return false;
            }

            await _userRepository.DeleteAsync(id);
            return true;
        }

        public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
        {
            var roles = await _roleRepository.GetAllAsync();
            return roles.Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description
            });
        }

        private async Task AssignRolesToUser(int userId, List<int> roleIds)
        {
            foreach (var roleId in roleIds)
            {
                await _userRepository.AssignRoleToUserAsync(userId, roleId);
            }
        }

        private UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                Roles = user.UserRoles?.Select(ur => new RoleDto
                {
                    Id = ur.Role.Id,
                    Name = ur.Role.Name,
                    Description = ur.Role.Description
                }).ToList() ?? new List<RoleDto>()
            };
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }
    }
}