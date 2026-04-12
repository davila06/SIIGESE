namespace Application.DTOs
{
    public class EmailAttachmentDto
    {
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = "application/octet-stream";
        public byte[] Content { get; set; } = Array.Empty<byte>();
    }

    public class EmailStats
    {
        public int TotalSent { get; set; }
        public int TotalFailed { get; set; }
        public int PendingCobros { get; set; }
        public int PolizasPorVencer { get; set; }
    }

    public class EmailRequestDto
    {
        public string ToEmail { get; set; } = string.Empty;
        public string? ToName { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string? EmailType { get; set; }
        public int? RelatedEntityId { get; set; }
        public string? RelatedEntityType { get; set; }
    }

    public class BulkEmailRequestDto
    {
        public List<EmailRequestDto> Emails { get; set; } = new();
    }

    public class CobroVencidoEmailRequestDto
    {
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteEmail { get; set; } = string.Empty;
        public string NumeroPoliza { get; set; } = string.Empty;
        public decimal MontoVencido { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public int DiasVencido { get; set; }
    }

    public class ReclamoRecibidoEmailRequestDto
    {
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteEmail { get; set; } = string.Empty;
        public string NumeroReclamo { get; set; } = string.Empty;
        public string NumeroPoliza { get; set; } = string.Empty;
        public DateTime FechaReclamo { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }

    public class BienvenidaEmailRequestDto
    {
        public string NombreUsuario { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string TemporalPassword { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
    }

    public class EmailResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? EmailLogId { get; set; }
    }

    public class EmailHistoryResponseDto
    {
        public int Id { get; set; }
        public string ToEmail { get; set; } = string.Empty;
        public string? ToName { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string EmailType { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public string SenderName { get; set; } = string.Empty;
    }
}
