using Application.DTOs.EmailConfig;
using Application.DTOs.Common;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Net;

namespace Application.Services
{
    public class EmailConfigService : IEmailConfigService
    {
        private readonly IEmailConfigRepository _repository;
        private readonly ILogger<EmailConfigService> _logger;

        public EmailConfigService(IEmailConfigRepository repository, ILogger<EmailConfigService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<List<EmailConfigResponseDto>>> GetAllAsync()
        {
            try
            {
                var configs = await _repository.GetAllAsync();
                var response = configs.Select(MapToResponseDto).ToList();
                
                return ApiResponse<List<EmailConfigResponseDto>>.CreateSuccess(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo configuraciones de email");
                return ApiResponse<List<EmailConfigResponseDto>>.CreateError("Error obteniendo configuraciones de email");
            }
        }

        public async Task<ApiResponse<EmailConfigResponseDto>> GetByIdAsync(int id)
        {
            try
            {
                var config = await _repository.GetByIdAsync(id);
                if (config == null)
                {
                    return ApiResponse<EmailConfigResponseDto>.CreateError("Configuración de email no encontrada");
                }

                var response = MapToResponseDto(config);
                return ApiResponse<EmailConfigResponseDto>.CreateSuccess(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo configuración de email con ID {Id}", id);
                return ApiResponse<EmailConfigResponseDto>.CreateError("Error obteniendo configuración de email");
            }
        }

        public async Task<ApiResponse<EmailConfigResponseDto>> GetDefaultAsync()
        {
            try
            {
                var config = await _repository.GetDefaultAsync();
                if (config == null)
                {
                    return ApiResponse<EmailConfigResponseDto>.CreateError("No hay configuración de email por defecto");
                }

                var response = MapToResponseDto(config);
                return ApiResponse<EmailConfigResponseDto>.CreateSuccess(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo configuración de email por defecto");
                return ApiResponse<EmailConfigResponseDto>.CreateError("Error obteniendo configuración de email por defecto");
            }
        }

        public async Task<ApiResponse<EmailConfigResponseDto>> CreateAsync(EmailConfigCreateDto dto, string createdBy)
        {
            try
            {
                // Verificar si ya existe una configuración con el mismo nombre
                if (await _repository.ExistsByNameAsync(dto.ConfigName))
                {
                    return ApiResponse<EmailConfigResponseDto>.CreateError("Ya existe una configuración con este nombre");
                }

                var config = new EmailConfig
                {
                    ConfigName = dto.ConfigName,
                    SmtpServer = dto.SmtpServer,
                    SmtpPort = dto.SmtpPort,
                    FromEmail = dto.FromEmail,
                    FromName = dto.FromName,
                    Username = dto.Username,
                    Password = dto.Password,
                    UseSSL = dto.UseSSL,
                    UseTLS = dto.UseTLS,
                    IsDefault = dto.IsDefault,
                    IsActive = dto.IsActive,
                    Description = dto.Description,
                    CompanyName = dto.CompanyName,
                    CompanyAddress = dto.CompanyAddress,
                    CompanyPhone = dto.CompanyPhone,
                    CompanyWebsite = dto.CompanyWebsite,
                    CompanyLogo = dto.CompanyLogo,
                    TimeoutSeconds = dto.TimeoutSeconds,
                    MaxRetries = dto.MaxRetries,
                    CreatedBy = createdBy,
                    UpdatedBy = createdBy
                };

                var createdConfig = await _repository.CreateAsync(config);
                var response = MapToResponseDto(createdConfig);

                return ApiResponse<EmailConfigResponseDto>.CreateSuccess(response, "Configuración de email creada exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando configuración de email");
                return ApiResponse<EmailConfigResponseDto>.CreateError("Error creando configuración de email");
            }
        }

        public async Task<ApiResponse<EmailConfigResponseDto>> UpdateAsync(int id, EmailConfigUpdateDto dto, string updatedBy)
        {
            try
            {
                var config = await _repository.GetByIdAsync(id);
                if (config == null)
                {
                    return ApiResponse<EmailConfigResponseDto>.CreateError("Configuración de email no encontrada");
                }

                config.ConfigName = dto.ConfigName;
                config.SmtpServer = dto.SmtpServer;
                config.SmtpPort = dto.SmtpPort;
                config.FromEmail = dto.FromEmail;
                config.FromName = dto.FromName;
                config.Username = dto.Username;
                if (!string.IsNullOrEmpty(dto.Password))
                {
                    config.Password = dto.Password;
                }
                config.UseSSL = dto.UseSSL;
                config.UseTLS = dto.UseTLS;
                config.IsDefault = dto.IsDefault;
                config.IsActive = dto.IsActive;
                config.Description = dto.Description;
                config.CompanyName = dto.CompanyName;
                config.CompanyAddress = dto.CompanyAddress;
                config.CompanyPhone = dto.CompanyPhone;
                config.CompanyWebsite = dto.CompanyWebsite;
                config.CompanyLogo = dto.CompanyLogo;
                config.TimeoutSeconds = dto.TimeoutSeconds;
                config.MaxRetries = dto.MaxRetries;
                config.UpdatedBy = updatedBy;

                var updatedConfig = await _repository.UpdateAsync(config);
                var response = MapToResponseDto(updatedConfig);

                return ApiResponse<EmailConfigResponseDto>.CreateSuccess(response, "Configuración de email actualizada exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando configuración de email con ID {Id}", id);
                return ApiResponse<EmailConfigResponseDto>.CreateError("Error actualizando configuración de email");
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            try
            {
                var config = await _repository.GetByIdAsync(id);
                if (config == null)
                {
                    return ApiResponse<bool>.CreateError("Configuración de email no encontrada");
                }

                if (config.IsDefault)
                {
                    return ApiResponse<bool>.CreateError("No se puede eliminar la configuración por defecto");
                }

                var deleted = await _repository.DeleteAsync(id);
                return ApiResponse<bool>.CreateSuccess(deleted, "Configuración de email eliminada exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando configuración de email con ID {Id}", id);
                return ApiResponse<bool>.CreateError("Error eliminando configuración de email");
            }
        }

        public async Task<ApiResponse<bool>> SetAsDefaultAsync(int id, string updatedBy)
        {
            try
            {
                var result = await _repository.SetAsDefaultAsync(id);
                return ApiResponse<bool>.CreateSuccess(result, "Configuración establecida como predeterminada");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error estableciendo configuración por defecto con ID {Id}", id);
                return ApiResponse<bool>.CreateError("Error estableciendo configuración por defecto");
            }
        }

        public async Task<ApiResponse<EmailTestResponseDto>> TestConfigurationAsync(EmailTestRequestDto dto)
        {
            try
            {
                var config = await _repository.GetByIdAsync(dto.ConfigId);
                if (config == null)
                {
                    return ApiResponse<EmailTestResponseDto>.CreateError("Configuración de email no encontrada");
                }

                var testResult = await SendTestEmail(config, dto.ToEmail, dto.Subject, dto.Body);
                return ApiResponse<EmailTestResponseDto>.CreateSuccess(testResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error probando configuración de email con ID {ConfigId}", dto.ConfigId);
                return ApiResponse<EmailTestResponseDto>.CreateError("Error probando configuración de email");
            }
        }

        public async Task<ApiResponse<bool>> ToggleActiveStatusAsync(int id, string updatedBy)
        {
            try
            {
                var result = await _repository.ToggleActiveStatusAsync(id);
                return ApiResponse<bool>.CreateSuccess(result, "Estado de configuración cambiado exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cambiando estado de configuración de email con ID {Id}", id);
                return ApiResponse<bool>.CreateError("Error cambiando estado de configuración de email");
            }
        }

        private const string DefaultCobroSubject = "SINSEG - Cobro Vencido: Póliza {NumeroPoliza}";
        private const string DefaultCobroBody = @"<!DOCTYPE html>
<html>
<head><meta charset='utf-8'><title>Cobro Vencido - SINSEG</title>
<style>body{{font-family:Arial,sans-serif;color:#333;}}.container{{max-width:600px;margin:0 auto;padding:20px;}}.header{{background:#c0392b;color:white;padding:20px;text-align:center;}}.content{{padding:20px;background:#f9f9f9;}}.footer{{text-align:center;padding:20px;font-size:12px;color:#666;}}</style></head>
<body><div class='container'>
  <div class='header'><h2>Aviso de Cobro Vencido</h2></div>
  <div class='content'>
    <p>Estimado/a <strong>{ClienteNombre}</strong>,</p>
    <p>Le informamos que tiene un cobro vencido con los siguientes detalles:</p>
    <ul>
      <li><strong>Póliza:</strong> {NumeroPoliza}</li>
      <li><strong>Monto vencido:</strong> {MontoVencido}</li>
      <li><strong>Fecha de vencimiento:</strong> {FechaVencimiento}</li>
      <li><strong>Días en mora:</strong> {DiasMora}</li>
    </ul>
    <p>Por favor, regularice su situación a la brevedad posible para evitar inconvenientes con su cobertura.</p>
  </div>
  <div class='footer'><p>© 2025 SINSEG - Sistema Integral de Administración de Seguros</p></div>
</div></body></html>";

        public async Task<ApiResponse<CobroEmailTemplateDto>> GetCobroTemplateAsync()
        {
            try
            {
                var config = await _repository.GetDefaultAsync();
                var dto = new CobroEmailTemplateDto
                {
                    Subject = config?.CobroEmailSubject,
                    Body = config?.CobroEmailBody,
                    DefaultSubject = DefaultCobroSubject,
                    DefaultBody = DefaultCobroBody
                };
                return ApiResponse<CobroEmailTemplateDto>.CreateSuccess(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo plantilla de email de cobros");
                return ApiResponse<CobroEmailTemplateDto>.CreateError("Error obteniendo plantilla de email de cobros");
            }
        }

        public async Task<ApiResponse<bool>> UpdateCobroTemplateAsync(CobroEmailTemplateUpdateDto dto, string updatedBy)
        {
            try
            {
                var config = await _repository.GetDefaultAsync();
                if (config == null)
                {
                    return ApiResponse<bool>.CreateError("No hay configuración de email por defecto");
                }

                config.CobroEmailSubject = string.IsNullOrWhiteSpace(dto.Subject) ? null : dto.Subject.Trim();
                config.CobroEmailBody = string.IsNullOrWhiteSpace(dto.Body) ? null : dto.Body.Trim();
                config.UpdatedBy = updatedBy;

                await _repository.UpdateAsync(config);
                return ApiResponse<bool>.CreateSuccess(true, "Plantilla de email de cobros actualizada exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando plantilla de email de cobros");
                return ApiResponse<bool>.CreateError("Error actualizando plantilla de email de cobros");
            }
        }

        private EmailConfigResponseDto MapToResponseDto(EmailConfig config)
        {
            return new EmailConfigResponseDto
            {
                Id = config.Id,
                ConfigName = config.ConfigName,
                SmtpServer = config.SmtpServer,
                SmtpPort = config.SmtpPort,
                FromEmail = config.FromEmail,
                FromName = config.FromName,
                Username = config.Username,
                UseSSL = config.UseSSL,
                UseTLS = config.UseTLS,
                IsDefault = config.IsDefault,
                IsActive = config.IsActive,
                Description = config.Description,
                CompanyName = config.CompanyName,
                CompanyAddress = config.CompanyAddress,
                CompanyPhone = config.CompanyPhone,
                CompanyWebsite = config.CompanyWebsite,
                CompanyLogo = config.CompanyLogo,
                TimeoutSeconds = config.TimeoutSeconds,
                MaxRetries = config.MaxRetries,
                LastTested = config.LastTested,
                LastTestSuccessful = config.LastTestSuccessful,
                LastTestError = config.LastTestError,
                CreatedAt = config.CreatedAt,
                UpdatedAt = config.UpdatedAt ?? config.CreatedAt,
                CreatedBy = config.CreatedBy,
                UpdatedBy = config.UpdatedBy ?? config.CreatedBy
            };
        }

        private async Task<EmailTestResponseDto> SendTestEmail(EmailConfig config, string toEmail, string subject, string body)
        {
            try
            {
                using var client = new SmtpClient(config.SmtpServer, config.SmtpPort);
                client.Credentials = new NetworkCredential(config.Username, config.Password);
                client.EnableSsl = config.UseSSL;
                client.Timeout = config.TimeoutSeconds * 1000;

                using var message = new MailMessage();
                message.From = new MailAddress(config.FromEmail, config.FromName);
                message.To.Add(toEmail);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                await client.SendMailAsync(message);

                return new EmailTestResponseDto
                {
                    Success = true,
                    Message = "Correo de prueba enviado exitosamente",
                    TestedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando correo de prueba");
                return new EmailTestResponseDto
                {
                    Success = false,
                    Message = "Error enviando correo de prueba",
                    ErrorDetails = ex.Message,
                    TestedAt = DateTime.UtcNow
                };
            }
        }
    }
}
