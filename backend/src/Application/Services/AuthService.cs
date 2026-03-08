using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IPasswordResetTokenRepository _tokenRepository;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;
        // Stores invalidated tokens mapped to their expiry time so expired entries can be purged
        private static readonly ConcurrentDictionary<string, DateTime> _invalidatedTokens = new ConcurrentDictionary<string, DateTime>();

        public AuthService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IPasswordResetTokenRepository tokenRepository,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _tokenRepository = tokenRepository;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            try
            {
                // Buscar usuario por email
                var user = await _userRepository.GetByEmailAsync(request.Email);
                
                if (user == null)
                {
                    _logger.LogWarning("Intento de login con email inexistente: {Email}", request.Email);
                    throw new UnauthorizedAccessException("Email o contraseña incorrectos");
                }

                // Verificar si el usuario está activo y no eliminado
                if (!user.IsActive || user.IsDeleted)
                {
                    _logger.LogWarning("Intento de login con usuario inactivo: {Email}", request.Email);
                    throw new UnauthorizedAccessException("Usuario inactivo");
                }

                // Verificar contraseña
                if (!VerifyPassword(request.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Contraseña incorrecta para usuario: {Email}", request.Email);
                    throw new UnauthorizedAccessException("Email o contraseña incorrectos");
                }

                // Obtener usuario con roles
                var userWithRoles = await _userRepository.GetUserWithRolesAsync(user.Id);
                var roleNames = userWithRoles?.UserRoles.Select(ur => ur.Role.Name).ToList() ?? new List<string>();
                var roleDtos = userWithRoles?.UserRoles.Select(ur => new RoleDto 
                { 
                    Id = ur.Role.Id, 
                    Name = ur.Role.Name,
                    Description = ur.Role.Description 
                }).ToList() ?? new List<RoleDto>();

                // Generar token JWT
                var token = GenerateJwtToken(user, roleNames);

                _logger.LogInformation("Login exitoso para usuario: {Email}", request.Email);

                return new LoginResponseDto
                {
                    Token = token,
                    User = new UserDto
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        IsActive = user.IsActive,
                        Roles = roleDtos
                    },
                    ExpiresAt = DateTime.UtcNow.AddHours(int.Parse(_configuration["Jwt:ExpirationHours"] ?? "8"))
                };
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el login para usuario: {Email}", request.Email);
                throw new Exception("Error al iniciar sesión", ex);
            }
        }

        public Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                // Purge expired entries to prevent unbounded growth
                var now = DateTime.UtcNow;
                foreach (var tokenKey in _invalidatedTokens.Keys)
                {
                    if (_invalidatedTokens.TryGetValue(tokenKey, out var exp) && exp <= now)
                        _invalidatedTokens.TryRemove(tokenKey, out _);
                }

                // Verificar si el token ha sido invalidado
                if (_invalidatedTokens.TryGetValue(token, out _))
                {
                    return Task.FromResult(false);
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret no configurado"));

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token inválido");
                return Task.FromResult(false);
            }
        }

        public async Task LogoutAsync(string token)
        {
            // Store token with its expiry so it can be purged later
            var expirationHours = int.Parse(_configuration["Jwt:ExpirationHours"] ?? "8");
            _invalidatedTokens[token] = DateTime.UtcNow.AddHours(expirationHours);
            await Task.CompletedTask;
        }

        public async Task ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new InvalidOperationException("Usuario no encontrado");
                }

                // Verificar contraseña actual
                if (!VerifyPassword(currentPassword, user.PasswordHash))
                {
                    throw new UnauthorizedAccessException("Contraseña actual incorrecta");
                }

                // Validar nueva contraseña
                if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
                {
                    throw new InvalidOperationException("La nueva contraseña debe tener al menos 6 caracteres");
                }

                // Actualizar contraseña
                user.PasswordHash = HashPassword(newPassword);
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);

                _logger.LogInformation("Contraseña actualizada para usuario ID: {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar contraseña para usuario ID: {UserId}", userId);
                throw;
            }
        }

        public async Task ForgotPasswordAsync(string email)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(email);
                if (user == null)
                {
                    // Por seguridad, no revelar si el email existe o no
                    _logger.LogWarning("Solicitud de recuperación de contraseña para email inexistente: {Email}", email);
                    return;
                }

                // Generar token de recuperación
                var token = GenerateRandomToken();
                var resetToken = new PasswordResetToken
                {
                    UserId = user.Id,
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddHours(24),
                    IsUsed = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _tokenRepository.AddAsync(resetToken);

                // Enviar email con el token
                var resetLink = $"{_configuration["App:FrontendUrl"]}/reset-password?token={token}";
                await _emailService.SendPasswordResetEmailAsync(user.Email, user.FirstName, resetLink);

                _logger.LogInformation("Email de recuperación enviado a: {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar solicitud de recuperación de contraseña para: {Email}", email);
                throw;
            }
        }

        public async Task ResetPasswordAsync(string token, string newPassword)
        {
            try
            {
                // Buscar token
                var resetToken = await _tokenRepository.GetByTokenAsync(token);
                if (resetToken == null || resetToken.IsUsed || resetToken.ExpiresAt < DateTime.UtcNow)
                {
                    throw new InvalidOperationException("Token inválido o expirado");
                }

                // Validar nueva contraseña
                if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
                {
                    throw new InvalidOperationException("La contraseña debe tener al menos 6 caracteres");
                }

                // Actualizar contraseña
                var user = await _userRepository.GetByIdAsync(resetToken.UserId);
                if (user == null)
                {
                    throw new InvalidOperationException("Usuario no encontrado");
                }

                user.PasswordHash = HashPassword(newPassword);
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);

                // Marcar token como usado
                resetToken.IsUsed = true;
                await _tokenRepository.UpdateAsync(resetToken);

                _logger.LogInformation("Contraseña restablecida para usuario ID: {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al restablecer contraseña");
                throw;
            }
        }

        public async Task<LoginResponseDto> RefreshTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret no configurado"));

                // Validar el token actual
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = false, // No validamos expiración aquí porque queremos renovar tokens expirados
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                
                // Obtener el ID del usuario del token
                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    throw new UnauthorizedAccessException("Token inválido");
                }

                // Obtener el usuario actual
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null || !user.IsActive || user.IsDeleted)
                {
                    throw new UnauthorizedAccessException("Usuario no encontrado o inactivo");
                }

                // Obtener roles actuales del usuario
                var userWithRoles = await _userRepository.GetUserWithRolesAsync(user.Id);
                var roleNames = userWithRoles?.UserRoles.Select(ur => ur.Role.Name).ToList() ?? new List<string>();
                var roleDtos = userWithRoles?.UserRoles.Select(ur => new RoleDto 
                { 
                    Id = ur.Role.Id, 
                    Name = ur.Role.Name,
                    Description = ur.Role.Description 
                }).ToList() ?? new List<RoleDto>();

                // Generar nuevo token
                var newToken = GenerateJwtToken(user, roleNames);

                _logger.LogInformation("Token renovado exitosamente para usuario: {Email}", user.Email);

                return new LoginResponseDto
                {
                    Token = newToken,
                    User = new UserDto
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        IsActive = user.IsActive,
                        Roles = roleDtos
                    },
                    ExpiresAt = DateTime.UtcNow.AddHours(int.Parse(_configuration["Jwt:ExpirationHours"] ?? "8"))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al renovar token");
                throw new UnauthorizedAccessException("No se pudo renovar el token");
            }
        }

        private string GenerateJwtToken(User user, List<string> roles)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret no configurado"));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Agregar roles como claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var expirationHours = int.Parse(_configuration["Jwt:ExpirationHours"] ?? "8");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(expirationHours),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private static bool VerifyPassword(string password, string hash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hash;
        }

        private static string GenerateRandomToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }
    }
}
