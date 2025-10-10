namespace Application.DTOs.EmailConfig
{
    public class EmailConfigResponseDto
    {
        public int Id { get; set; }
        public string ConfigName { get; set; } = string.Empty;
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public bool UseSSL { get; set; }
        public bool UseTLS { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyAddress { get; set; }
        public string? CompanyPhone { get; set; }
        public string? CompanyWebsite { get; set; }
        public string? CompanyLogo { get; set; }
        public int TimeoutSeconds { get; set; }
        public int MaxRetries { get; set; }
        public DateTime LastTested { get; set; }
        public bool LastTestSuccessful { get; set; }
        public string? LastTestError { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
    }
}