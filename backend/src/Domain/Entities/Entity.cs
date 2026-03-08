using System;

namespace Domain.Entities
{
    public abstract class Entity
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public string CreatedBy { get; set; } = string.Empty;
        public string? UpdatedBy { get; set; }
    }
}