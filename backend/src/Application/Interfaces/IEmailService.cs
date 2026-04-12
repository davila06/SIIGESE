using Application.DTOs;

namespace Application.Interfaces
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string email, string resetToken, string resetUrl);
        Task SendWelcomeEmailAsync(string email, string firstName, string temporaryPassword);
        Task SendCobroVencidoNotificationAsync(CobroVencidoDto cobro);
        Task SendPolizaVencimientoNotificationAsync(PolizaVencimientoDto poliza);
        Task SendGenericEmailAsync(string toEmail, string subject, string body);
        Task SendGenericEmailWithAttachmentsAsync(string toEmail, string subject, string body, IEnumerable<EmailAttachmentDto> attachments);
        Task<bool> IsConfiguredAsync();
    }
}