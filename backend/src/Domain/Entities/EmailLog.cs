using System;

namespace Domain.Entities
{
    public class EmailLog : Entity
    {
        public string ToEmail { get; set; } = string.Empty;
        public string? ToName { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string EmailType { get; set; } = "Generic";
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public string SenderName { get; set; } = "SINSEG";
    }
}