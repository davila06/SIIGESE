using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.EmailConfig
{
    public class EmailConfigUpdateDto
    {
        [Required(ErrorMessage = "El nombre de configuración es requerido")]
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string ConfigName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El servidor SMTP es requerido")]
        [MaxLength(100, ErrorMessage = "El servidor SMTP no puede exceder 100 caracteres")]
        public string SmtpServer { get; set; } = string.Empty;

        [Required(ErrorMessage = "El puerto SMTP es requerido")]
        [Range(1, 65535, ErrorMessage = "El puerto debe estar entre 1 y 65535")]
        public int SmtpPort { get; set; } = 587;

        [Required(ErrorMessage = "El email de origen es requerido")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [MaxLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
        public string FromEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre de origen es requerido")]
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string FromName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [MaxLength(100, ErrorMessage = "El usuario no puede exceder 100 caracteres")]
        public string Username { get; set; } = string.Empty;

        [MaxLength(200, ErrorMessage = "La contraseña no puede exceder 200 caracteres")]
        public string? Password { get; set; } // Opcional en actualización

        public bool UseSSL { get; set; } = true;

        public bool UseTLS { get; set; } = true;

        public bool IsDefault { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [MaxLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string? Description { get; set; }

        [MaxLength(200, ErrorMessage = "El nombre de la empresa no puede exceder 200 caracteres")]
        public string? CompanyName { get; set; }

        [MaxLength(500, ErrorMessage = "La dirección no puede exceder 500 caracteres")]
        public string? CompanyAddress { get; set; }

        [MaxLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
        public string? CompanyPhone { get; set; }

        [MaxLength(100, ErrorMessage = "El sitio web no puede exceder 100 caracteres")]
        public string? CompanyWebsite { get; set; }

        [MaxLength(500, ErrorMessage = "La URL del logo no puede exceder 500 caracteres")]
        public string? CompanyLogo { get; set; }

        [Range(5, 300, ErrorMessage = "El timeout debe estar entre 5 y 300 segundos")]
        public int TimeoutSeconds { get; set; } = 30;

        [Range(0, 10, ErrorMessage = "Los reintentos deben estar entre 0 y 10")]
        public int MaxRetries { get; set; } = 3;
    }
}