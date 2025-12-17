using Application.DTOs;

namespace Application.Services
{
    public interface INotificationService
    {
        Task<NotificationResultDto> ProcessOverduePaymentsAsync();
        Task<NotificationResultDto> ProcessExpiringPoliciesAsync(int daysBeforeExpiration = 30);
        Task<NotificationResultDto> ProcessAllNotificationsAsync(int daysBeforeExpiration = 30);
        Task<List<CobroVencidoDto>> GetOverduePaymentsAsync();
        Task<List<PolizaVencimientoDto>> GetExpiringPoliciesAsync(int daysBeforeExpiration = 30);
        Task<NotificationStatisticsDto> GetNotificationStatisticsAsync(int daysBeforeExpiration = 30);
    }
}
