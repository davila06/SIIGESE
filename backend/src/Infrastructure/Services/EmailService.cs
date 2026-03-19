using System.Net;
using System.Net.Mail;
using Application.DTOs;
using Application.Interfaces;
using Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly IEmailConfigRepository _emailConfigRepository;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger, IEmailConfigRepository emailConfigRepository)
        {
            _configuration = configuration;
            _logger = logger;
            _emailConfigRepository = emailConfigRepository;
        }

        public async Task SendPasswordResetEmailAsync(string email, string resetToken, string resetUrl)
        {
            var subject = "SINSEG - Restablecer Contraseña";
            var body = GeneratePasswordResetEmailBody(resetUrl, resetToken);

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendWelcomeEmailAsync(string email, string firstName, string temporaryPassword)
        {
            var subject = "SINSEG - Bienvenido al Sistema";
            var body = GenerateWelcomeEmailBody(firstName, email, temporaryPassword);

            await SendEmailAsync(email, subject, body);
        }

        public async Task<bool> IsConfiguredAsync()
        {
            var smtpServer = _configuration["Email:SmtpServer"];
            var smtpUser = _configuration["Email:SmtpUser"];
            
            return await Task.FromResult(!string.IsNullOrEmpty(smtpServer) && !string.IsNullOrEmpty(smtpUser));
        }

        public async Task SendCobroVencidoNotificationAsync(CobroVencidoDto cobro)
        {
            if (string.IsNullOrWhiteSpace(cobro.ClienteEmail))
                return;

            string subject;
            string body;

            try
            {
                var config = await _emailConfigRepository.GetDefaultAsync();
                subject = !string.IsNullOrWhiteSpace(config?.CobroEmailSubject)
                    ? config.CobroEmailSubject.Replace("{NumeroPoliza}", cobro.NumeroPoliza)
                    : $"SINSEG - Cobro Vencido: Póliza {cobro.NumeroPoliza}";

                body = !string.IsNullOrWhiteSpace(config?.CobroEmailBody)
                    ? ApplyCobroTemplateVariables(config.CobroEmailBody, cobro)
                    : GenerateCobroVencidoEmailBody(cobro);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo obtener plantilla de cobro, usando plantilla por defecto");
                subject = $"SINSEG - Cobro Vencido: Póliza {cobro.NumeroPoliza}";
                body = GenerateCobroVencidoEmailBody(cobro);
            }

            await SendEmailAsync(cobro.ClienteEmail, subject, body);
        }

        private string ApplyCobroTemplateVariables(string template, CobroVencidoDto cobro)
        {
            return template
                .Replace("{ClienteNombre}", cobro.ClienteNombre)
                .Replace("{NumeroPoliza}", cobro.NumeroPoliza)
                .Replace("{MontoVencido}", cobro.MontoVencido.ToString("C"))
                .Replace("{FechaVencimiento}", cobro.FechaVencimiento.ToString("dd/MM/yyyy"))
                .Replace("{DiasMora}", cobro.DiasMora.ToString());
        }

        public async Task SendPolizaVencimientoNotificationAsync(PolizaVencimientoDto poliza)
        {
            if (string.IsNullOrWhiteSpace(poliza.ClienteEmail))
                return;

            var subject = $"SINSEG - Póliza por Vencer: {poliza.NumeroPoliza}";
            var body = GeneratePolizaVencimientoEmailBody(poliza);
            await SendEmailAsync(poliza.ClienteEmail, subject, body);
        }

        private async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                // Verificar configuración
                var smtpServer = _configuration["Email:SmtpServer"];
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var smtpUser = _configuration["Email:SmtpUser"];
                var smtpPassword = _configuration["Email:SmtpPassword"];
                var fromEmail = _configuration["Email:FromEmail"];
                var fromName = _configuration["Email:FromName"] ?? "SINSEG";

                if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpUser))
                {
                    _logger.LogWarning("Email no configurado. Email no enviado a {Email}", to);
                    return;
                }

                using var client = new SmtpClient(smtpServer, smtpPort);
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(smtpUser, smtpPassword);

                var message = new MailMessage
                {
                    From = new MailAddress(fromEmail ?? smtpUser, fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                message.To.Add(to);

                await client.SendMailAsync(message);
                _logger.LogInformation("Email enviado exitosamente a {Email}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando email a {Email}", to);
                throw;
            }
        }

        private string GeneratePasswordResetEmailBody(string resetUrl, string resetToken)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Restablecer Contraseña - SINSEG</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background: #f9f9f9; }}
        .button {{ display: inline-block; background: #667eea; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔒 SINSEG</h1>
            <h2>Restablecer Contraseña</h2>
        </div>
        <div class='content'>
            <p>Hola,</p>
            <p>Has solicitado restablecer tu contraseña en el Sistema Integral de Administración de Seguros (SINSEG).</p>
            <p>Haz clic en el siguiente enlace para crear una nueva contraseña:</p>
            <p style='text-align: center;'>
                <a href='{resetUrl}?token={resetToken}' class='button'>Restablecer Contraseña</a>
            </p>
            <p><strong>Importante:</strong></p>
            <ul>
                <li>Este enlace es válido por 1 hora</li>
                <li>Solo puede ser usado una vez</li>
                <li>Si no solicitaste este cambio, puedes ignorar este email</li>
            </ul>
            <p>Por tu seguridad, nunca compartas este enlace con nadie.</p>
        </div>
        <div class='footer'>
            <p>© 2025 SINSEG - Sistema Integral de Administración de Seguros</p>
            <p>Este es un email automático, por favor no responder.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateWelcomeEmailBody(string firstName, string email, string temporaryPassword)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Bienvenido a SINSEG</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background: #f9f9f9; }}
        .credentials {{ background: #e8f4f8; padding: 15px; border-left: 4px solid #667eea; margin: 20px 0; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 ¡Bienvenido a SINSEG!</h1>
            <h2>Sistema Integral de Administración de Seguros</h2>
        </div>
        <div class='content'>
            <p>Hola {firstName},</p>
            <p>¡Bienvenido al Sistema Integral de Administración de Seguros! Tu cuenta ha sido creada exitosamente.</p>
            
            <div class='credentials'>
                <h3>📋 Credenciales de Acceso:</h3>
                <p><strong>Email:</strong> {email}</p>
                <p><strong>Contraseña Temporal:</strong> {temporaryPassword}</p>
            </div>

            <p><strong>⚠️ Importante:</strong></p>
            <ul>
                <li>Debes cambiar tu contraseña en el primer inicio de sesión</li>
                <li>Asegúrate de usar una contraseña segura</li>
                <li>Nunca compartas tus credenciales con terceros</li>
            </ul>

            <p>Puedes acceder al sistema en: <a href='http://localhost:4200'>http://localhost:4200</a></p>
        </div>
        <div class='footer'>
            <p>© 2025 SINSEG - Sistema Integral de Administración de Seguros</p>
            <p>Si tienes problemas, contacta al administrador del sistema.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateCobroVencidoEmailBody(CobroVencidoDto cobro)
        {
            return $@"<!DOCTYPE html>
<html>
<head><meta charset='utf-8'><title>Cobro Vencido - SINSEG</title>
<style>body{{font-family:Arial,sans-serif;color:#333;}}.container{{max-width:600px;margin:0 auto;padding:20px;}}.header{{background:#c0392b;color:white;padding:20px;text-align:center;}}.content{{padding:20px;background:#f9f9f9;}}.footer{{text-align:center;padding:20px;font-size:12px;color:#666;}}</style></head>
<body><div class='container'>
  <div class='header'><h2>Aviso de Cobro Vencido</h2></div>
  <div class='content'>
    <p>Estimado/a <strong>{cobro.ClienteNombre}</strong>,</p>
    <p>Le informamos que tiene un cobro vencido con los siguientes detalles:</p>
    <ul>
      <li><strong>Póliza:</strong> {cobro.NumeroPoliza}</li>
      <li><strong>Monto vencido:</strong> {cobro.MontoVencido:C}</li>
      <li><strong>Fecha de vencimiento:</strong> {cobro.FechaVencimiento:dd/MM/yyyy}</li>
      <li><strong>Días en mora:</strong> {cobro.DiasMora}</li>
    </ul>
    <p>Por favor, regularice su situación a la brevedad posible para evitar inconvenientes con su cobertura.</p>
  </div>
  <div class='footer'><p>© 2025 SINSEG - Sistema Integral de Administración de Seguros</p></div>
</div></body></html>";
        }

        private string GeneratePolizaVencimientoEmailBody(PolizaVencimientoDto poliza)
        {
            return $@"<!DOCTYPE html>
<html>
<head><meta charset='utf-8'><title>Póliza por Vencer - SINSEG</title>
<style>body{{font-family:Arial,sans-serif;color:#333;}}.container{{max-width:600px;margin:0 auto;padding:20px;}}.header{{background:#e67e22;color:white;padding:20px;text-align:center;}}.content{{padding:20px;background:#f9f9f9;}}.footer{{text-align:center;padding:20px;font-size:12px;color:#666;}}</style></head>
<body><div class='container'>
  <div class='header'><h2>Aviso de Vencimiento de Póliza</h2></div>
  <div class='content'>
    <p>Estimado/a <strong>{poliza.ClienteNombre}</strong>,</p>
    <p>Le informamos que su póliza está próxima a vencer:</p>
    <ul>
      <li><strong>Número de póliza:</strong> {poliza.NumeroPoliza}</li>
      <li><strong>Tipo:</strong> {poliza.TipoPoliza}</li>
      <li><strong>Fecha de vencimiento:</strong> {poliza.FechaVencimiento:dd/MM/yyyy}</li>
      <li><strong>Días hasta el vencimiento:</strong> {poliza.DiasHastaVencimiento}</li>
      <li><strong>Prima:</strong> {poliza.Prima:C}</li>
    </ul>
    <p>Le recomendamos contactar a su asesor para gestionar la renovación.</p>
  </div>
  <div class='footer'><p>© 2025 SINSEG - Sistema Integral de Administración de Seguros</p></div>
</div></body></html>";
        }

        public async Task SendGenericEmailAsync(string toEmail, string subject, string body)
        {
            await SendEmailAsync(toEmail, subject, body);
        }
    }
}