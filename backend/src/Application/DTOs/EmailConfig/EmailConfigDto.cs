namespace Application.DTOs.EmailConfig
{
    public class EmailConfigDto
    {
        public int Id { get; set; }
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string SmtpUser { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public bool EnableSsl { get; set; }
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastTested { get; set; }
        public bool? LastTestSuccessful { get; set; }
        public string? LastTestError { get; set; }
        public int TimeoutSeconds { get; set; }
        public int MaxRetries { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class EmailConfigResponseDto
    {
        public int Id { get; set; }
        public string ConfigName { get; set; } = string.Empty;
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public bool UseSSL { get; set; }
        public bool UseTLS { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public string Description { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyAddress { get; set; } = string.Empty;
        public string CompanyPhone { get; set; } = string.Empty;
        public string CompanyWebsite { get; set; } = string.Empty;
        public string CompanyLogo { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; }
        public int MaxRetries { get; set; }
        public DateTime? LastTested { get; set; }
        public bool LastTestSuccessful { get; set; }
        public string? LastTestError { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string? UpdatedBy { get; set; }
    }

    public class EmailConfigCreateDto
    {
        public string ConfigName { get; set; } = string.Empty;
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool UseSSL { get; set; }
        public bool UseTLS { get; set; }
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; } = true;
        public string Description { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyAddress { get; set; } = string.Empty;
        public string CompanyPhone { get; set; } = string.Empty;
        public string CompanyWebsite { get; set; } = string.Empty;
        public string CompanyLogo { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; } = 30;
        public int MaxRetries { get; set; } = 3;
    }

    public class EmailConfigUpdateDto
    {
        public string ConfigName { get; set; } = string.Empty;
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool UseSSL { get; set; }
        public bool UseTLS { get; set; }
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public string Description { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyAddress { get; set; } = string.Empty;
        public string CompanyPhone { get; set; } = string.Empty;
        public string CompanyWebsite { get; set; } = string.Empty;
        public string CompanyLogo { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; }
        public int MaxRetries { get; set; }
    }

    public class EmailTestRequestDto
    {
        public int ConfigId { get; set; }
        public string ToEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = "Prueba de Configuración de Email";
        public string Body { get; set; } = "Este es un email de prueba para verificar la configuración.";
    }

    public class EmailTestResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime TestedAt { get; set; } = DateTime.UtcNow;
        public int ResponseTimeMs { get; set; }
        public string? ErrorDetails { get; set; }
    }

    public class CreateEmailConfigDto
    {
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string SmtpUser { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public bool EnableSsl { get; set; }
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class UpdateEmailConfigDto
    {
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string SmtpUser { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public bool EnableSsl { get; set; }
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class CobroEmailTemplateDto
    {
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public string DefaultSubject { get; set; } = string.Empty;
        public string DefaultBody { get; set; } = string.Empty;
    }

    public class CobroEmailTemplateUpdateDto
    {
        public string? Subject { get; set; }
        public string? Body { get; set; }
    }
}