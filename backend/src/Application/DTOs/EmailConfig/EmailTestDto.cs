namespace Application.DTOs.EmailConfig
{
    public class EmailTestRequestDto
    {
        public int ConfigId { get; set; }
        public string ToEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = "Prueba de configuración SMTP";
        public string Body { get; set; } = "Este es un correo de prueba para verificar la configuración SMTP.";
    }

    public class EmailTestResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime TestedAt { get; set; }
        public string? ErrorDetails { get; set; }
    }
}