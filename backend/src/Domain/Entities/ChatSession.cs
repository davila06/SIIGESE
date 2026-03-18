using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public enum ChatSessionStatus
    {
        Active = 1,
        Closed = 2,
        Archived = 3
    }

    public class ChatSession : Entity
    {
        public string SessionId { get; set; } = Guid.NewGuid().ToString("N");
        public int UserId { get; set; }
        public string Title { get; set; } = "Nueva conversación";
        public ChatSessionStatus Status { get; set; } = ChatSessionStatus.Active;
        public string? LastMessage { get; set; }
        public int MessageCount { get; set; } = 0;
        public DateTime? LastActivityAt { get; set; }

        // Navigation
        public virtual User? User { get; set; }
        public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}
