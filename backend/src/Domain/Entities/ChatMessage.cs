namespace Domain.Entities
{
    public enum ChatMessageType
    {
        User = 1,
        Bot = 2,
        System = 3
    }

    public enum ChatMessageStatus
    {
        Sent = 1,
        Read = 2,
        Error = 3
    }

    public class ChatMessage : Entity
    {
        public int ChatSessionId { get; set; }
        public string Content { get; set; } = string.Empty;
        public ChatMessageType MessageType { get; set; } = ChatMessageType.User;
        public ChatMessageStatus Status { get; set; } = ChatMessageStatus.Sent;

        /// <summary>
        /// JSON-serialized rich content: cards, tables, lists (null for plain text)
        /// </summary>
        public string? RichContent { get; set; }

        /// <summary>
        /// JSON array of quick-reply strings shown below bot messages
        /// </summary>
        public string? QuickReplies { get; set; }

        /// <summary>
        /// User reaction: 1 = like, -1 = dislike, null = no reaction
        /// </summary>
        public int? ReactionScore { get; set; }

        public bool IsRead { get; set; } = false;

        /// <summary>
        /// Milliseconds the AI took to generate the response
        /// </summary>
        public int? ProcessingTimeMs { get; set; }

        // Navigation
        public virtual ChatSession? ChatSession { get; set; }
    }
}
