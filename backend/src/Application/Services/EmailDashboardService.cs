using Application.DTOs;
using Application.DTOs.Common;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class EmailDashboardService : IEmailDashboardService
    {
        private readonly IEmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EmailDashboardService> _logger;

        public EmailDashboardService(
            IEmailService emailService,
            IUnitOfWork unitOfWork,
            ILogger<EmailDashboardService> logger)
        {
            _emailService = emailService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<EmailStats>> GetStatsAsync()
        {
            try
            {
                var pendingCobros = await _unitOfWork.Cobros.GetCobrosVencidosCountAsync();

                var polizasActivas = await _unitOfWork.Polizas.GetActivasAsync();
                var today = DateTime.Today;
                var in30Days = today.AddDays(30);
                var polizasPorVencer = polizasActivas
                    .Count(p => p.FechaVigencia >= today && p.FechaVigencia <= in30Days);

                var totalSent = await _unitOfWork.EmailLogs.GetSuccessCountAsync();
                var totalFailed = await _unitOfWork.EmailLogs.GetFailedCountAsync();

                var stats = new EmailStats
                {
                    TotalSent = totalSent,
                    TotalFailed = totalFailed,
                    PendingCobros = pendingCobros,
                    PolizasPorVencer = polizasPorVencer
                };

                return ApiResponse<EmailStats>.CreateSuccess(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo estadísticas de email");
                return ApiResponse<EmailStats>.CreateError("Error obteniendo estadísticas de email");
            }
        }

        public async Task<ApiResponse<EmailResponseDto>> SendEmailAsync(EmailRequestDto request)
        {
            try
            {
                await _emailService.SendGenericEmailAsync(request.ToEmail, request.Subject, request.Body);

                var logId = await PersistEmailLogAsync(new EmailLog
                {
                    ToEmail = request.ToEmail,
                    ToName = request.ToName,
                    Subject = request.Subject,
                    Body = request.Body,
                    EmailType = string.IsNullOrWhiteSpace(request.EmailType) ? "Generic" : request.EmailType,
                    IsSuccess = true,
                    SentAt = DateTime.UtcNow,
                    SenderName = "SINSEG"
                });

                return ApiResponse<EmailResponseDto>.CreateSuccess(new EmailResponseDto
                {
                    IsSuccess = true,
                    Message = $"Email enviado exitosamente a {request.ToEmail}",
                    EmailLogId = logId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando email a {Email}", request.ToEmail);

                var logId = await PersistEmailLogAsync(new EmailLog
                {
                    ToEmail = request.ToEmail,
                    ToName = request.ToName,
                    Subject = request.Subject,
                    Body = request.Body,
                    EmailType = string.IsNullOrWhiteSpace(request.EmailType) ? "Generic" : request.EmailType,
                    IsSuccess = false,
                    ErrorMessage = ex.Message,
                    SentAt = DateTime.UtcNow,
                    SenderName = "SINSEG"
                });

                return ApiResponse<EmailResponseDto>.CreateSuccess(new EmailResponseDto
                {
                    IsSuccess = false,
                    Message = $"Error enviando email: {ex.Message}",
                    EmailLogId = logId
                });
            }
        }

        public async Task<ApiResponse<List<EmailResponseDto>>> SendBulkEmailsAsync(BulkEmailRequestDto request)
        {
            var results = new List<EmailResponseDto>();

            foreach (var email in request.Emails)
            {
                try
                {
                    await _emailService.SendGenericEmailAsync(email.ToEmail, email.Subject, email.Body);
                    var logId = await PersistEmailLogAsync(new EmailLog
                    {
                        ToEmail = email.ToEmail,
                        ToName = email.ToName,
                        Subject = email.Subject,
                        Body = email.Body,
                        EmailType = string.IsNullOrWhiteSpace(email.EmailType) ? "Bulk" : email.EmailType,
                        IsSuccess = true,
                        SentAt = DateTime.UtcNow,
                        SenderName = "SINSEG"
                    });

                    results.Add(new EmailResponseDto { IsSuccess = true, Message = $"Enviado a {email.ToEmail}", EmailLogId = logId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error enviando email masivo a {Email}", email.ToEmail);
                    var logId = await PersistEmailLogAsync(new EmailLog
                    {
                        ToEmail = email.ToEmail,
                        ToName = email.ToName,
                        Subject = email.Subject,
                        Body = email.Body,
                        EmailType = string.IsNullOrWhiteSpace(email.EmailType) ? "Bulk" : email.EmailType,
                        IsSuccess = false,
                        ErrorMessage = ex.Message,
                        SentAt = DateTime.UtcNow,
                        SenderName = "SINSEG"
                    });

                    results.Add(new EmailResponseDto { IsSuccess = false, Message = $"Error: {ex.Message}", EmailLogId = logId });
                }
            }

            return ApiResponse<List<EmailResponseDto>>.CreateSuccess(results);
        }

        public async Task<ApiResponse<EmailResponseDto>> SendCobroVencidoEmailAsync(CobroVencidoEmailRequestDto request)
        {
            try
            {
                var dto = new CobroVencidoDto
                {
                    NumeroPoliza = request.NumeroPoliza,
                    ClienteEmail = request.ClienteEmail,
                    ClienteNombre = request.ClienteNombre,
                    MontoVencido = request.MontoVencido,
                    FechaVencimiento = request.FechaVencimiento,
                    DiasMora = request.DiasVencido
                };

                await _emailService.SendCobroVencidoNotificationAsync(dto);

                var logId = await PersistEmailLogAsync(new EmailLog
                {
                    ToEmail = request.ClienteEmail,
                    ToName = request.ClienteNombre,
                    Subject = $"SINSEG - Cobro Vencido: Póliza {request.NumeroPoliza}",
                    Body = string.Empty,
                    EmailType = "CobroVencido",
                    IsSuccess = true,
                    SentAt = DateTime.UtcNow,
                    SenderName = "SINSEG"
                });

                return ApiResponse<EmailResponseDto>.CreateSuccess(new EmailResponseDto
                {
                    IsSuccess = true,
                    Message = $"Notificación de cobro vencido enviada a {request.ClienteEmail}",
                    EmailLogId = logId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando notificación de cobro vencido");

                var logId = await PersistEmailLogAsync(new EmailLog
                {
                    ToEmail = request.ClienteEmail,
                    ToName = request.ClienteNombre,
                    Subject = $"SINSEG - Cobro Vencido: Póliza {request.NumeroPoliza}",
                    Body = string.Empty,
                    EmailType = "CobroVencido",
                    IsSuccess = false,
                    ErrorMessage = ex.Message,
                    SentAt = DateTime.UtcNow,
                    SenderName = "SINSEG"
                });

                return ApiResponse<EmailResponseDto>.CreateSuccess(new EmailResponseDto
                {
                    IsSuccess = false,
                    Message = $"Error: {ex.Message}",
                    EmailLogId = logId
                });
            }
        }

        public async Task<ApiResponse<EmailResponseDto>> SendReclamoRecibidoEmailAsync(ReclamoRecibidoEmailRequestDto request)
        {
            try
            {
                var subject = $"SINSEG - Reclamo Recibido: {request.NumeroReclamo}";
                var body = GenerateReclamoRecibidoEmailBody(request);
                await _emailService.SendGenericEmailAsync(request.ClienteEmail, subject, body);

                var logId = await PersistEmailLogAsync(new EmailLog
                {
                    ToEmail = request.ClienteEmail,
                    ToName = request.ClienteNombre,
                    Subject = subject,
                    Body = body,
                    EmailType = "ReclamoRecibido",
                    IsSuccess = true,
                    SentAt = DateTime.UtcNow,
                    SenderName = "SINSEG"
                });

                return ApiResponse<EmailResponseDto>.CreateSuccess(new EmailResponseDto
                {
                    IsSuccess = true,
                    Message = $"Notificación de reclamo enviada a {request.ClienteEmail}",
                    EmailLogId = logId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando notificación de reclamo recibido");

                var logId = await PersistEmailLogAsync(new EmailLog
                {
                    ToEmail = request.ClienteEmail,
                    ToName = request.ClienteNombre,
                    Subject = $"SINSEG - Reclamo Recibido: {request.NumeroReclamo}",
                    Body = string.Empty,
                    EmailType = "ReclamoRecibido",
                    IsSuccess = false,
                    ErrorMessage = ex.Message,
                    SentAt = DateTime.UtcNow,
                    SenderName = "SINSEG"
                });

                return ApiResponse<EmailResponseDto>.CreateSuccess(new EmailResponseDto
                {
                    IsSuccess = false,
                    Message = $"Error: {ex.Message}",
                    EmailLogId = logId
                });
            }
        }

        public async Task<ApiResponse<EmailResponseDto>> SendBienvenidaEmailAsync(BienvenidaEmailRequestDto request)
        {
            try
            {
                await _emailService.SendWelcomeEmailAsync(request.Email, request.NombreUsuario, request.TemporalPassword);

                var logId = await PersistEmailLogAsync(new EmailLog
                {
                    ToEmail = request.Email,
                    ToName = request.NombreUsuario,
                    Subject = "SINSEG - Bienvenido al Sistema",
                    Body = string.Empty,
                    EmailType = "Bienvenida",
                    IsSuccess = true,
                    SentAt = DateTime.UtcNow,
                    SenderName = "SINSEG"
                });

                return ApiResponse<EmailResponseDto>.CreateSuccess(new EmailResponseDto
                {
                    IsSuccess = true,
                    Message = $"Email de bienvenida enviado a {request.Email}",
                    EmailLogId = logId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando email de bienvenida");

                var logId = await PersistEmailLogAsync(new EmailLog
                {
                    ToEmail = request.Email,
                    ToName = request.NombreUsuario,
                    Subject = "SINSEG - Bienvenido al Sistema",
                    Body = string.Empty,
                    EmailType = "Bienvenida",
                    IsSuccess = false,
                    ErrorMessage = ex.Message,
                    SentAt = DateTime.UtcNow,
                    SenderName = "SINSEG"
                });

                return ApiResponse<EmailResponseDto>.CreateSuccess(new EmailResponseDto
                {
                    IsSuccess = false,
                    Message = $"Error: {ex.Message}",
                    EmailLogId = logId
                });
            }
        }

        public async Task<ApiResponse<List<EmailHistoryResponseDto>>> GetEmailHistoryAsync(int pageNumber, int pageSize)
        {
            try
            {
                var logs = await _unitOfWork.EmailLogs.GetPagedAsync(pageNumber, pageSize);
                var history = logs.Select(log => new EmailHistoryResponseDto
                {
                    Id = log.Id,
                    ToEmail = log.ToEmail,
                    ToName = log.ToName,
                    Subject = log.Subject,
                    EmailType = log.EmailType,
                    SentAt = log.SentAt,
                    IsSuccess = log.IsSuccess,
                    ErrorMessage = log.ErrorMessage,
                    SenderName = log.SenderName
                }).ToList();

                return ApiResponse<List<EmailHistoryResponseDto>>.CreateSuccess(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo historial de emails");
                return ApiResponse<List<EmailHistoryResponseDto>>.CreateError("No se pudo obtener el historial de emails");
            }
        }

        public async Task<ApiResponse<EmailResponseDto>> ResendEmailAsync(int id)
        {
            if (id <= 0)
            {
                return ApiResponse<EmailResponseDto>.CreateSuccess(new EmailResponseDto
                {
                    IsSuccess = false,
                    Message = "El id del email a reenviar no es válido."
                });
            }

            try
            {
                var emailLog = await _unitOfWork.EmailLogs.GetByIdAsync(id);
                if (emailLog == null)
                {
                    return ApiResponse<EmailResponseDto>.CreateSuccess(new EmailResponseDto
                    {
                        IsSuccess = false,
                        Message = $"No existe registro de email para id {id}."
                    });
                }

                if (string.IsNullOrWhiteSpace(emailLog.Body))
                {
                    return ApiResponse<EmailResponseDto>.CreateSuccess(new EmailResponseDto
                    {
                        IsSuccess = false,
                        Message = "El email original no contiene cuerpo para reenvío."
                    });
                }

                await _emailService.SendGenericEmailAsync(emailLog.ToEmail, emailLog.Subject, emailLog.Body);

                var resendLogId = await PersistEmailLogAsync(new EmailLog
                {
                    ToEmail = emailLog.ToEmail,
                    ToName = emailLog.ToName,
                    Subject = emailLog.Subject,
                    Body = emailLog.Body,
                    EmailType = string.IsNullOrWhiteSpace(emailLog.EmailType) ? "Resend" : $"Resend-{emailLog.EmailType}",
                    IsSuccess = true,
                    SentAt = DateTime.UtcNow,
                    SenderName = emailLog.SenderName
                });

                return ApiResponse<EmailResponseDto>.CreateSuccess(new EmailResponseDto
                {
                    IsSuccess = true,
                    Message = $"Email reenviado exitosamente a {emailLog.ToEmail}.",
                    EmailLogId = resendLogId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reenviando email con id {EmailLogId}", id);

                var failedResendLogId = await PersistEmailLogAsync(new EmailLog
                {
                    ToEmail = string.Empty,
                    Subject = $"Resend-{id}",
                    Body = string.Empty,
                    EmailType = "Resend",
                    IsSuccess = false,
                    ErrorMessage = ex.Message,
                    SentAt = DateTime.UtcNow,
                    SenderName = "SINSEG"
                });

                return ApiResponse<EmailResponseDto>.CreateSuccess(new EmailResponseDto
                {
                    IsSuccess = false,
                    Message = $"No se pudo reenviar el email con id {id}: {ex.Message}",
                    EmailLogId = failedResendLogId
                });
            }
        }

        public async Task<ApiResponse<List<EmailResponseDto>>> SendAutomaticCobroVencidoNotificationsAsync()
        {
            var results = new List<EmailResponseDto>();

            try
            {
                var cobrosVencidos = await _unitOfWork.Cobros.GetCobrosVencidosAsync();

                foreach (var cobro in cobrosVencidos)
                {
                    if (string.IsNullOrWhiteSpace(cobro.CorreoElectronico))
                        continue;

                    try
                    {
                        var dto = new CobroVencidoDto
                        {
                            CobroId = cobro.Id,
                            NumeroPoliza = cobro.NumeroPoliza,
                            ClienteEmail = cobro.CorreoElectronico,
                            ClienteNombre = cobro.ClienteNombreCompleto,
                            MontoVencido = cobro.MontoTotal,
                            FechaVencimiento = cobro.FechaVencimiento,
                            DiasMora = (int)(DateTime.Today - cobro.FechaVencimiento).TotalDays
                        };

                        await _emailService.SendCobroVencidoNotificationAsync(dto);
                        results.Add(new EmailResponseDto
                        {
                            IsSuccess = true,
                            Message = $"Notificación enviada a {cobro.CorreoElectronico} (Póliza {cobro.NumeroPoliza})"
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error enviando notificación de cobro vencido para póliza {Poliza}", cobro.NumeroPoliza);
                        results.Add(new EmailResponseDto
                        {
                            IsSuccess = false,
                            Message = $"Error para póliza {cobro.NumeroPoliza}: {ex.Message}"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo cobros vencidos para notificaciones automáticas");
            }

            return ApiResponse<List<EmailResponseDto>>.CreateSuccess(results,
                $"Proceso completado: {results.Count(r => r.IsSuccess)} enviados, {results.Count(r => !r.IsSuccess)} fallidos");
        }

        public async Task<ApiResponse<List<EmailResponseDto>>> SendAutomaticPolizasPorVencerNotificationsAsync()
        {
            var results = new List<EmailResponseDto>();

            try
            {
                var polizasActivas = await _unitOfWork.Polizas.GetActivasAsync();
                var today = DateTime.Today;
                var in30Days = today.AddDays(30);

                var polizasPorVencer = polizasActivas
                    .Where(p => p.FechaVigencia >= today && p.FechaVigencia <= in30Days)
                    .ToList();

                foreach (var poliza in polizasPorVencer)
                {
                    if (string.IsNullOrWhiteSpace(poliza.Correo))
                        continue;

                    try
                    {
                        var dto = new PolizaVencimientoDto
                        {
                            PolizaId = poliza.Id,
                            NumeroPoliza = poliza.NumeroPoliza ?? string.Empty,
                            ClienteEmail = poliza.Correo,
                            ClienteNombre = poliza.NombreAsegurado ?? string.Empty,
                            FechaVencimiento = poliza.FechaVigencia,
                            DiasHastaVencimiento = (int)(poliza.FechaVigencia - today).TotalDays,
                            TipoPoliza = poliza.Modalidad ?? string.Empty,
                            MontoAsegurado = 0,
                            Prima = poliza.Prima
                        };

                        await _emailService.SendPolizaVencimientoNotificationAsync(dto);
                        results.Add(new EmailResponseDto
                        {
                            IsSuccess = true,
                            Message = $"Notificación enviada a {poliza.Correo} (Póliza {poliza.NumeroPoliza})"
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error enviando notificación de vencimiento para póliza {Poliza}", poliza.NumeroPoliza);
                        results.Add(new EmailResponseDto
                        {
                            IsSuccess = false,
                            Message = $"Error para póliza {poliza.NumeroPoliza}: {ex.Message}"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo pólizas por vencer para notificaciones automáticas");
            }

            return ApiResponse<List<EmailResponseDto>>.CreateSuccess(results,
                $"Proceso completado: {results.Count(r => r.IsSuccess)} enviados, {results.Count(r => !r.IsSuccess)} fallidos");
        }

        private static string GenerateReclamoRecibidoEmailBody(ReclamoRecibidoEmailRequestDto data)
        {
            return $@"<!DOCTYPE html>
<html>
<head><meta charset='utf-8'><title>Reclamo Recibido - SINSEG</title>
<style>body{{font-family:Arial,sans-serif;color:#333;}}.container{{max-width:600px;margin:0 auto;padding:20px;}}.header{{background:#1976d2;color:white;padding:20px;text-align:center;}}.content{{padding:20px;background:#f9f9f9;}}.footer{{text-align:center;padding:20px;font-size:12px;color:#666;}}</style>
</head>
<body><div class='container'>
  <div class='header'><h2>Confirmación de Reclamo</h2></div>
  <div class='content'>
    <p>Estimado/a <strong>{data.ClienteNombre}</strong>,</p>
    <p>Hemos recibido su reclamo y será atendido a la brevedad posible.</p>
    <ul>
      <li><strong>Número de reclamo:</strong> {data.NumeroReclamo}</li>
      <li><strong>Póliza:</strong> {data.NumeroPoliza}</li>
      <li><strong>Fecha:</strong> {data.FechaReclamo:dd/MM/yyyy}</li>
      <li><strong>Descripción:</strong> {data.Descripcion}</li>
    </ul>
    <p>Nos pondremos en contacto con usted pronto.</p>
  </div>
  <div class='footer'><p>© 2025 SINSEG - Sistema Integral de Administración de Seguros</p></div>
</div></body></html>";
        }

        private async Task<int?> PersistEmailLogAsync(EmailLog log)
        {
            try
            {
                log.CreatedAt = DateTime.UtcNow;
                log.CreatedBy = "EmailDashboardService";
                await _unitOfWork.EmailLogs.AddAsync(log);
                await _unitOfWork.SaveChangesAsync();
                return log.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo persistir el email log para destinatario {ToEmail}", log.ToEmail);
                return null;
            }
        }
    }
}
