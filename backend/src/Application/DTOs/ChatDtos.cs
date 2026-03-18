using System;
using System.Collections.Generic;

namespace Application.DTOs
{
    public class ChatSessionDto
    {
        public int Id { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? LastMessage { get; set; }
        public int MessageCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastActivityAt { get; set; }
        public int UnreadCount { get; set; }
    }

    public class ChatMessageDto
    {
        public int Id { get; set; }
        public int ChatSessionId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string MessageType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public object? RichContent { get; set; }
        public List<string>? QuickReplies { get; set; }
        public int? ReactionScore { get; set; }
        public bool IsRead { get; set; }
        public int? ProcessingTimeMs { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateChatSessionDto
    {
        public string? Title { get; set; }
    }

    public class SendMessageDto
    {
        public string Content { get; set; } = string.Empty;
    }

    public class SendMessageResponseDto
    {
        public ChatMessageDto UserMessage { get; set; } = new();
        public ChatMessageDto BotResponse { get; set; } = new();
        public string SessionId { get; set; } = string.Empty;
        public string SessionTitle { get; set; } = string.Empty;
    }

    public class ChatSessionDetailDto
    {
        public ChatSessionDto Session { get; set; } = new();
        public List<ChatMessageDto> Messages { get; set; } = new();
    }

    public class ReactToMessageDto
    {
        /// <summary>1 = like, -1 = dislike</summary>
        public int Score { get; set; }
    }

    public class ChatStatsDto
    {
        public int TotalSessions { get; set; }
        public int TotalMessages { get; set; }
        public int ActiveSessions { get; set; }
        public double AvgMessagesPerSession { get; set; }
        public double AvgResponseTimeMs { get; set; }
        public int LikedResponses { get; set; }
        public int DislikedResponses { get; set; }
    }
}
