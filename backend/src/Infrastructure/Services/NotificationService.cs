using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            ApplicationDbContext context,
            IEmailService emailService,
            ILogger<NotificationService> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<NotificationResultDto> ProcessOverduePaymentsAsync()
        {
            var result = new NotificationResultDto
            {
                Success = true,
                Message = "Procesamiento completado"
            };

            try
            {
                var overduePayments = await GetOverduePaymentsAsync();

                foreach (var cobro in overduePayments)
                {
                    try
                    {
                        await _emailService.SendCobroVencidoNotificationAsync(cobro);
                        result.OverduePaymentsSent++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error enviando notificación de cobro vencido {cobro.CobroId}");
                        result.OverduePaymentsFailed++;
                    }
                }

                result.Success = result.OverduePaymentsFailed == 0;
                result.Message = $"Enviadas: {result.OverduePaymentsSent}, Fallidas: {result.OverduePaymentsFailed}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando cobros vencidos");
                result.Success = false;
                result.Message = $"Error: {ex.Message}";
            }

            return result;
        }

        public async Task<NotificationResultDto> ProcessExpiringPoliciesAsync(int daysBeforeExpiration = 30)
        {
            var result = new NotificationResultDto
            {
                Success = true,
                Message = "Procesamiento completado"
            };

            try
            {
                var expiringPolicies = await GetExpiringPoliciesAsync(daysBeforeExpiration);

                foreach (var poliza in expiringPolicies)
                {
                    try
                    {
                        await _emailService.SendPolizaVencimientoNotificationAsync(poliza);
                        result.ExpiringPoliciesSent++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error enviando notificación de póliza por vencer {poliza.PolizaId}");
                        result.ExpiringPoliciesFailed++;
                    }
                }

                result.Success = result.ExpiringPoliciesFailed == 0;
                result.Message = $"Enviadas: {result.ExpiringPoliciesSent}, Fallidas: {result.ExpiringPoliciesFailed}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando pólizas por vencer");
                result.Success = false;
                result.Message = $"Error: {ex.Message}";
            }

            return result;
        }

        public async Task<NotificationResultDto> ProcessAllNotificationsAsync(int daysBeforeExpiration = 30)
        {
            var overdueResult = await ProcessOverduePaymentsAsync();
            var expiringResult = await ProcessExpiringPoliciesAsync(daysBeforeExpiration);

            return new NotificationResultDto
            {
                Success = overdueResult.Success && expiringResult.Success,
                Message = $"Cobros: {overdueResult.Message}, Pólizas: {expiringResult.Message}",
                OverduePaymentsSent = overdueResult.OverduePaymentsSent,
                OverduePaymentsFailed = overdueResult.OverduePaymentsFailed,
                ExpiringPoliciesSent = expiringResult.ExpiringPoliciesSent,
                ExpiringPoliciesFailed = expiringResult.ExpiringPoliciesFailed
            };
        }

        public async Task<List<CobroVencidoDto>> GetOverduePaymentsAsync()
        {
            var today = DateTime.Today;

            var overdueCobros = await _context.Cobros
                .Where(c => !c.IsDeleted 
                    && c.Estado == EstadoCobro.Pendiente 
                    && c.FechaVencimiento < today)
                .OrderByDescending(c => c.FechaVencimiento)
                .Select(c => new CobroVencidoDto
                {
                    CobroId = c.Id,
                    NumeroPoliza = c.NumeroPoliza,
                    ClienteEmail = c.CorreoElectronico ?? "",
                    ClienteNombre = c.ClienteNombreCompleto,
                    MontoVencido = c.MontoTotal,
                    FechaVencimiento = c.FechaVencimiento,
                    DiasMora = (today - c.FechaVencimiento).Days,
                    Concepto = c.Observaciones // Usar Observaciones como Concepto
                })
                .ToListAsync();

            return overdueCobros;
        }

        public async Task<List<PolizaVencimientoDto>> GetExpiringPoliciesAsync(int daysBeforeExpiration = 30)
        {
            var today = DateTime.Today;
            var expirationDate = today.AddDays(daysBeforeExpiration);

            var expiringPolizas = await _context.Polizas
                .Where(p => !p.IsDeleted 
                    && p.FechaVigencia >= today 
                    && p.FechaVigencia <= expirationDate)
                .OrderBy(p => p.FechaVigencia)
                .Select(p => new PolizaVencimientoDto
                {
                    PolizaId = p.Id,
                    NumeroPoliza = p.NumeroPoliza ?? "",
                    ClienteEmail = p.Correo ?? "",
                    ClienteNombre = p.NombreAsegurado ?? "",
                    FechaVencimiento = p.FechaVigencia,
                    DiasHastaVencimiento = (p.FechaVigencia - today).Days,
                    TipoPoliza = p.Modalidad ?? "", // Usar Modalidad como TipoPoliza
                    MontoAsegurado = 0, // Poliza no tiene este campo
                    Prima = p.Prima
                })
                .ToListAsync();

            return expiringPolizas;
        }

        public async Task<NotificationStatisticsDto> GetNotificationStatisticsAsync(int daysBeforeExpiration = 30)
        {
            var overduePayments = await GetOverduePaymentsAsync();
            var expiringPolicies = await GetExpiringPoliciesAsync(daysBeforeExpiration);

            return new NotificationStatisticsDto
            {
                OverduePaymentsCount = overduePayments.Count,
                ExpiringPoliciesCount = expiringPolicies.Count,
                TotalOverdueAmount = overduePayments.Sum(c => c.MontoVencido),
                TotalInsuredAmount = expiringPolicies.Sum(p => p.MontoAsegurado),
                DaysBeforeExpiration = daysBeforeExpiration,
                LastUpdated = DateTime.Now
            };
        }
    }
}
