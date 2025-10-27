using System;

namespace Domain.Entities
{
    public class PasswordResetToken : Entity
    {
        public int UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; } = false;
        public DateTime? UsedAt { get; set; }
        
        // Navigation properties
        public virtual User User { get; set; } = null!;
    }
}
