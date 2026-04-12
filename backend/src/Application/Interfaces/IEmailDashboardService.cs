using Application.DTOs;
using Application.DTOs.Common;

namespace Application.Interfaces
{
    public interface IEmailDashboardService
    {
        Task<ApiResponse<EmailStats>> GetStatsAsync();
        Task<ApiResponse<EmailResponseDto>> SendEmailAsync(EmailRequestDto request);
        Task<ApiResponse<List<EmailResponseDto>>> SendBulkEmailsAsync(BulkEmailRequestDto request);
        Task<ApiResponse<EmailResponseDto>> SendCobroVencidoEmailAsync(CobroVencidoEmailRequestDto request);
        Task<ApiResponse<EmailResponseDto>> SendReclamoRecibidoEmailAsync(ReclamoRecibidoEmailRequestDto request);
        Task<ApiResponse<EmailResponseDto>> SendBienvenidaEmailAsync(BienvenidaEmailRequestDto request);
        Task<ApiResponse<List<EmailHistoryResponseDto>>> GetEmailHistoryAsync(int pageNumber, int pageSize);
        Task<ApiResponse<EmailResponseDto>> ResendEmailAsync(int id);
        Task<ApiResponse<List<EmailResponseDto>>> SendAutomaticCobroVencidoNotificationsAsync();
        Task<ApiResponse<List<EmailResponseDto>>> SendAutomaticPolizasPorVencerNotificationsAsync();
    }
}
