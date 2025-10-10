using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class EmailConfig : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string ConfigName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string SmtpServer { get; set; } = string.Empty;

        [Required]
        [Range(1, 65535)]
        public int SmtpPort { get; set; } = 587;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string FromEmail { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FromName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Password { get; set; } = string.Empty;

        public bool UseSSL { get; set; } = true;

        public bool UseTLS { get; set; } = true;

        public bool IsDefault { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string? Description { get; set; }

        // Configuraciones adicionales para plantillas
        [MaxLength(200)]
        public string? CompanyName { get; set; }

        [MaxLength(500)]
        public string? CompanyAddress { get; set; }

        [MaxLength(20)]
        public string? CompanyPhone { get; set; }

        [MaxLength(100)]
        public string? CompanyWebsite { get; set; }

        [MaxLength(500)]
        public string? CompanyLogo { get; set; }

        // Configuraciones de timeouts y reintento
        public int TimeoutSeconds { get; set; } = 30;

        public int MaxRetries { get; set; } = 3;

        public DateTime LastTested { get; set; } = DateTime.UtcNow;

        public bool LastTestSuccessful { get; set; } = false;

        [MaxLength(500)]
        public string? LastTestError { get; set; }
    }
}