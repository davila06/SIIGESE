using System;

namespace Domain.Entities
{
    public class DataRecord : Entity
    {
        public string FileName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public int TotalRecords { get; set; }
        public int ProcessedRecords { get; set; }
        public int ErrorRecords { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Processing, Completed, Failed
        public DateTime ProcessedAt { get; set; }
        public string? ErrorDetails { get; set; }
        public int UploadedByUserId { get; set; }
        public int PerfilId { get; set; }

        // Navigation properties
        public virtual User UploadedBy { get; set; } = null!;
    }
}
