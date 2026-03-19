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
        private readonly ICobroRepository _cobroRepository;
        private readonly IPolizaRepository _polizaRepository;
        private readonly ILogger<EmailDashboardService> _logger;

        public EmailDashboardService(
            IEmailService emailService,
            ICobroRepository cobroRepository,
            IPolizaRepository polizaRepository,
            ILogger<EmailDashboardService> logger)
        {
            _emailService = emailService;
            _cobroRepository = cobroRepository;
            _polizaRepository = polizaRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<EmailStats>> GetStatsAsync()
        {
            try
            {
                var pendingCobros = await _cobroRepository.GetCobrosVencidosCountAsync();

                var polizasActivas = await _polizaRepository.GetActivasAsync();
                var today = DateTime.Today;
                var in30Days = today.AddDays(30);
                var polizasPorVencer = polizasActivas
                    .Count(p => p.FechaVigencia >= today && p.FechaVigencia <= in30Days);

                var stats = new EmailStats
                {
                    TotalSent = 0,
                    TotalFailed = 0,
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
                return ApiResponse<EmailResponseDto>.CreateSuccess(new EmailResponseDto
                {
                    IsSuccess = true,
                    Message = $"Email enviado exitosamente a {request.ToEmail}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando email a {Email}", request.ToEmail);
                return ApiResponse<EmailResponseDto>.CreateSuccess(new EmailResponseDto
                {
                    IsSuccess = false,
                    Message = $"Error enviando email: {ex.Message}"
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
                    results.Add(new EmailResponseDto { IsSuccess = true, Message = $"Enviado a {email.ToEmail}" });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error enviando email masivo a {Email}", email.ToEmail);
                    results.Add(new EmailResponseDto { IsSuccess = false, Message = $"Error: {ex.Message}" });
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

                return ApiResponse<EmailResponseDto>.CreateSuccess(new EmailResponseDto
                {
                    IsSuccess = true,
                    Message = $"Notificación de cobro vencido enviada a {request.ClienteEmail}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando notificación de cobro vencido");
                return ApiResponse<EmailResponseDto>.CreateSuccess(new EmailResponseDto
                {
                    IsSuccess = false,
                    Message = $"Error: {ex.Message}"
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

                return ApiResponse<EmailResponseDto>.CreateSuccess(new EmailResponseDto
                {
                    IsSuccess = true,
                    Message = $"Notificación de reclamo enviada a {request.ClienteEmail}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando notificación de reclamo recibido");
                return ApiResponse<EmailResponseDto>.CreateSuccess(new EmailResponseDto
                {
                    IsSuccess = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        public async Task<ApiResponse<EmailResponseDto>> SendBienvenidaEmailAsync(BienvenidaEmailRequestDto request)
        {
            try
            {
                await _emailService.SendWelcomeEmailAsync(request.Email, request.NombreUsuario, request.TemporalPassword);

                return ApiResponse<EmailResponseDto>.CreateSuccess(new EmailResponseDto
                {
                    IsSuccess = true,
                    Message = $"Email de bienvenida enviado a {request.Email}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando email de bienvenida");
                return ApiResponse<EmailResponseDto>.CreateSuccess(new EmailResponseDto
                {
                    IsSuccess = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        public Task<ApiResponse<List<EmailHistoryResponseDto>>> GetEmailHistoryAsync(int pageNumber, int pageSize)
        {
            // No hay tabla de historial de emails todavía; retorna lista vacía
            var history = new List<EmailHistoryResponseDto>();
            return Task.FromResult(ApiResponse<List<EmailHistoryResponseDto>>.CreateSuccess(history, "Sin historial almacenado"));
        }

        public async Task<ApiResponse<List<EmailResponseDto>>> SendAutomaticCobroVencidoNotificationsAsync()
        {
            var results = new List<EmailResponseDto>();

            try
            {
                var cobrosVencidos = await _cobroRepository.GetCobrosVencidosAsync();

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
                var polizasActivas = await _polizaRepository.GetActivasAsync();
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
    }
}
