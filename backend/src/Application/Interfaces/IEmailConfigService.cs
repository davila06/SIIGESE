using Application.DTOs.EmailConfig;
using Application.DTOs.Common;

namespace Application.Interfaces
{
    public interface IEmailConfigService
    {
        Task<ApiResponse<List<EmailConfigResponseDto>>> GetAllAsync();
        Task<ApiResponse<EmailConfigResponseDto>> GetByIdAsync(int id);
        Task<ApiResponse<EmailConfigResponseDto>> GetDefaultAsync();
        Task<ApiResponse<EmailConfigResponseDto>> CreateAsync(EmailConfigCreateDto dto, string createdBy);
        Task<ApiResponse<EmailConfigResponseDto>> UpdateAsync(int id, EmailConfigUpdateDto dto, string updatedBy);
        Task<ApiResponse<bool>> DeleteAsync(int id);
        Task<ApiResponse<bool>> SetAsDefaultAsync(int id, string updatedBy);
        Task<ApiResponse<EmailTestResponseDto>> TestConfigurationAsync(EmailTestRequestDto dto);
        Task<ApiResponse<bool>> ToggleActiveStatusAsync(int id, string updatedBy);
        Task<ApiResponse<CobroEmailTemplateDto>> GetCobroTemplateAsync();
        Task<ApiResponse<bool>> UpdateCobroTemplateAsync(CobroEmailTemplateUpdateDto dto, string updatedBy);
    }
}