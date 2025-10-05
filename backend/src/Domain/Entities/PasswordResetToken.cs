using System;

namespace Domain.Entities
{
    public class PasswordResetToken : BaseEntity
    {
        public string Token { get; set; } = string.Empty;
        public int UserId { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; } = false;
        public DateTime? UsedAt { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        
        // Navigation property
        public virtual User User { get; set; } = null!;
    }
}