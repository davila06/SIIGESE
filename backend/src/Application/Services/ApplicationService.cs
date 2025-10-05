using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Linq;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using AutoMapper;
using Application.DTOs;
using Application.Interfaces;
using Domain.Interfaces;
using BCrypt.Net;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using ClosedXML.Excel;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
            
            if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Credenciales inválidas");
            }

            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException("Usuario inactivo");
            }

            var userWithRoles = await _unitOfWork.Users.GetUserWithRolesAsync(user.Id);
            var token = GenerateJwtToken(userWithRoles!);

            user.LastLoginAt = DateTime.UtcNow;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return new LoginResponseDto
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(8),
                User = _mapper.Map<UserDto>(userWithRoles),
                RequiresPasswordChange = user.RequiresPasswordChange
            };
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"] ?? "default-secret-key");
                
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task LogoutAsync(string token)
        {
            // Implementar blacklist de tokens si es necesario
            await Task.CompletedTask;
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"] ?? "default-secret-key");
            
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.UserName)
            };

            foreach (var userRole in user.UserRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private bool VerifyPassword(string password, string hash)
        {
            // Implementar verificación de password con BCrypt
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        public async Task ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException("Usuario no encontrado");
            }

            // Verificar contraseña actual
            if (!VerifyPassword(currentPassword, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Contraseña actual incorrecta");
            }

            // Validar nueva contraseña
            ValidatePassword(newPassword);

            // Encriptar nueva contraseña
            user.PasswordHash = HashPassword(newPassword);
            user.RequiresPasswordChange = false; // Ya no requiere cambio
            user.LastPasswordChangeAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ForgotPasswordAsync(string email)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(email);
            if (user == null)
            {
                throw new KeyNotFoundException("Usuario no encontrado");
            }

            // Invalidar tokens existentes del usuario
            await _unitOfWork.PasswordResetTokens.InvalidateUserTokensAsync(user.Id);

            // Generar nuevo token de reset (válido por 1 hora)
            var resetToken = Guid.NewGuid().ToString().Replace("-", "");
            var expiresAt = DateTime.UtcNow.AddHours(1);

            var passwordResetToken = new PasswordResetToken
            {
                Token = resetToken,
                UserId = user.Id,
                ExpiresAt = expiresAt,
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.PasswordResetTokens.AddAsync(passwordResetToken);
            await _unitOfWork.SaveChangesAsync();

            // Enviar email con token de reset
            var resetUrl = _configuration["App:FrontendUrl"] ?? "http://localhost:4200";
            resetUrl += "/change-password";
            
            try
            {
                await _emailService.SendPasswordResetEmailAsync(email, resetToken, resetUrl);
            }
            catch (Exception ex)
            {
                // Log el error pero no fallar el proceso
                // En un entorno de producción, podrías querer fallar si el email no se puede enviar
                Console.WriteLine($"Error enviando email: {ex.Message}");
            }
        }

        public async Task ResetPasswordAsync(string token, string newPassword)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedAccessException("Token inválido");
            }

            // Buscar token válido en base de datos
            var resetToken = await _unitOfWork.PasswordResetTokens.GetByTokenAsync(token);
            if (resetToken == null)
            {
                throw new UnauthorizedAccessException("Token inválido o expirado");
            }

            // Validar nueva contraseña
            ValidatePassword(newPassword);

            // Actualizar contraseña del usuario
            var user = resetToken.User;
            user.PasswordHash = HashPassword(newPassword);
            user.RequiresPasswordChange = false; // Ya no requiere cambio
            user.LastPasswordChangeAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Users.UpdateAsync(user);

            // Marcar token como usado
            resetToken.IsUsed = true;
            resetToken.UsedAt = DateTime.UtcNow;
            await _unitOfWork.PasswordResetTokens.UpdateAsync(resetToken);

            await _unitOfWork.SaveChangesAsync();
        }

        private void ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("La contraseña es requerida");
            }

            if (password.Length < 8)
            {
                throw new ArgumentException("La contraseña debe tener al menos 8 caracteres");
            }

            if (!password.Any(char.IsUpper))
            {
                throw new ArgumentException("La contraseña debe contener al menos una mayúscula");
            }

            if (!password.Any(char.IsLower))
            {
                throw new ArgumentException("La contraseña debe contener al menos una minúscula");
            }

            if (!password.Any(char.IsDigit))
            {
                throw new ArgumentException("La contraseña debe contener al menos un número");
            }

            if (!password.Any(c => !char.IsLetterOrDigit(c)))
            {
                throw new ArgumentException("La contraseña debe contener al menos un carácter especial");
            }
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }

    public class ClienteService : IClienteService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ClienteService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ClienteDto>> GetAllAsync()
        {
            var clientes = await _unitOfWork.Clientes.GetActivosAsync();
            return _mapper.Map<IEnumerable<ClienteDto>>(clientes);
        }

        public async Task<IEnumerable<ClienteDto>> GetByPerfilIdAsync(int perfilId)
        {
            var clientes = await _unitOfWork.Clientes.GetByPerfilIdAsync(perfilId);
            return _mapper.Map<IEnumerable<ClienteDto>>(clientes);
        }

        public async Task<ClienteDto?> GetByIdAsync(int id)
        {
            var cliente = await _unitOfWork.Clientes.GetByIdAsync(id);
            return cliente != null ? _mapper.Map<ClienteDto>(cliente) : null;
        }

        public async Task<ClienteDto> CreateAsync(CreateClienteDto dto)
        {
            var existingCodigo = await _unitOfWork.Clientes.GetByCodigoAsync(dto.Codigo);
            if (existingCodigo != null)
                throw new InvalidOperationException("Ya existe un cliente con este código");

            var existingNIT = await _unitOfWork.Clientes.GetByNITAsync(dto.NIT);
            if (existingNIT != null)
                throw new InvalidOperationException("Ya existe un cliente con este NIT");

            var cliente = _mapper.Map<Cliente>(dto);
            await _unitOfWork.Clientes.AddAsync(cliente);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ClienteDto>(cliente);
        }

        public async Task<ClienteDto> UpdateAsync(int id, CreateClienteDto dto)
        {
            var cliente = await _unitOfWork.Clientes.GetByIdAsync(id);
            if (cliente == null)
                throw new KeyNotFoundException("Cliente no encontrado");

            _mapper.Map(dto, cliente);
            cliente.UpdatedAt = DateTime.UtcNow;
            
            await _unitOfWork.Clientes.UpdateAsync(cliente);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ClienteDto>(cliente);
        }

        public async Task DeleteAsync(int id)
        {
            var cliente = await _unitOfWork.Clientes.GetByIdAsync(id);
            if (cliente == null)
                throw new KeyNotFoundException("Cliente no encontrado");

            cliente.IsDeleted = true;
            cliente.EsActivo = false;
            cliente.UpdatedAt = DateTime.UtcNow;
            
            await _unitOfWork.Clientes.UpdateAsync(cliente);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<DataUploadResultDto> ProcesarExcelAsync(int perfilId, IFormFile file, int userId)
        {
            var result = new DataUploadResultDto();
            var errors = new List<string>();
            var clientes = new List<Cliente>();

            // Crear registro de carga
            var dataRecord = new DataRecord
            {
                FileName = file.FileName,
                FileType = file.ContentType,
                FileSize = file.Length,
                UploadedByUserId = userId,
                PerfilId = perfilId,
                Status = "Processing",
                ProcessedAt = DateTime.UtcNow
            };

            await _unitOfWork.DataRecords.AddAsync(dataRecord);
            await _unitOfWork.SaveChangesAsync();

            try
            {
                using var stream = file.OpenReadStream();
                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheet(1);
                
                var rows = worksheet.RowsUsed().Skip(1); // Saltar header
                result.TotalRecords = rows.Count();

                foreach (var row in rows)
                {
                    try
                    {
                        var cliente = new Cliente
                        {
                            Codigo = row.Cell(1).GetString(),
                            RazonSocial = row.Cell(2).GetString(),
                            NombreComercial = row.Cell(3).GetString(),
                            NIT = row.Cell(4).GetString(),
                            Telefono = row.Cell(5).GetString(),
                            Email = row.Cell(6).GetString(),
                            Direccion = row.Cell(7).GetString(),
                            Ciudad = row.Cell(8).GetString(),
                            Departamento = row.Cell(9).GetString(),
                            Pais = row.Cell(10).GetString(),
                            PerfilId = perfilId,
                            CreatedBy = userId.ToString(),
                            FechaRegistro = DateTime.UtcNow
                        };

                        // Validaciones básicas
                        if (string.IsNullOrEmpty(cliente.Codigo) || string.IsNullOrEmpty(cliente.RazonSocial) || string.IsNullOrEmpty(cliente.NIT))
                        {
                            errors.Add($"Fila {row.RowNumber()}: Campos obligatorios vacíos");
                            result.ErrorRecords++;
                            continue;
                        }

                        // Verificar duplicados
                        var existingCodigo = await _unitOfWork.Clientes.GetByCodigoAsync(cliente.Codigo);
                        var existingNIT = await _unitOfWork.Clientes.GetByNITAsync(cliente.NIT);
                        
                        if (existingCodigo != null || existingNIT != null)
                        {
                            errors.Add($"Fila {row.RowNumber()}: Cliente ya existe (Código: {cliente.Codigo}, NIT: {cliente.NIT})");
                            result.ErrorRecords++;
                            continue;
                        }

                        clientes.Add(cliente);
                        result.ProcessedRecords++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Fila {row.RowNumber()}: {ex.Message}");
                        result.ErrorRecords++;
                    }
                }

                if (clientes.Any())
                {
                    await _unitOfWork.Clientes.AddRangeAsync(clientes);
                    await _unitOfWork.SaveChangesAsync();
                }

                // Actualizar registro de carga
                dataRecord.TotalRecords = result.TotalRecords;
                dataRecord.ProcessedRecords = result.ProcessedRecords;
                dataRecord.ErrorRecords = result.ErrorRecords;
                dataRecord.Status = result.ErrorRecords > 0 ? "Completed with errors" : "Completed";
                dataRecord.ErrorDetails = string.Join(";", errors);
                
                await _unitOfWork.DataRecords.UpdateAsync(dataRecord);
                await _unitOfWork.SaveChangesAsync();

                result.Status = dataRecord.Status;
                result.Errors = errors;
            }
            catch (Exception ex)
            {
                dataRecord.Status = "Failed";
                dataRecord.ErrorDetails = ex.Message;
                await _unitOfWork.DataRecords.UpdateAsync(dataRecord);
                await _unitOfWork.SaveChangesAsync();

                throw new InvalidOperationException($"Error procesando archivo Excel: {ex.Message}");
            }

            return result;
        }
    }
}