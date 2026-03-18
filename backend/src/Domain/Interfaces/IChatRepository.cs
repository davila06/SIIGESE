using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IChatRepository
    {
        // Sessions
        Task<ChatSession?> GetSessionBySessionIdAsync(string sessionId, int userId);
        Task<IEnumerable<ChatSession>> GetSessionsByUserAsync(int userId);
        Task<ChatSession> AddSessionAsync(ChatSession session);
        Task UpdateSessionAsync(ChatSession session);
        Task DeleteSessionAsync(ChatSession session);

        // Messages
        Task<IEnumerable<ChatMessage>> GetMessagesBySessionAsync(int chatSessionId, int skip = 0, int take = 50);
        Task<ChatMessage> AddMessageAsync(ChatMessage message);
        Task UpdateMessageAsync(ChatMessage message);
        Task<ChatMessage?> GetMessageByIdAsync(int messageId);
        Task MarkSessionMessagesReadAsync(int chatSessionId, int userId);
        Task<int> GetUnreadCountAsync(int chatSessionId);
    }
}
