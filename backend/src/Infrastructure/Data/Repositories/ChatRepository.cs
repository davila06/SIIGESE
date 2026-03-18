using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Domain.Interfaces;

namespace Infrastructure.Data.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly ApplicationDbContext _context;

        public ChatRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ── Sessions ────────────────────────────────────────────────────────────

        public async Task<ChatSession?> GetSessionBySessionIdAsync(string sessionId, int userId)
        {
            return await _context.ChatSessions
                .Where(s => s.SessionId == sessionId && s.UserId == userId && !s.IsDeleted)
                .Include(s => s.Messages.Where(m => !m.IsDeleted))
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ChatSession>> GetSessionsByUserAsync(int userId)
        {
            return await _context.ChatSessions
                .Where(s => s.UserId == userId && !s.IsDeleted)
                .OrderByDescending(s => s.LastActivityAt ?? s.CreatedAt)
                .Take(30)
                .ToListAsync();
        }

        public async Task<ChatSession> AddSessionAsync(ChatSession session)
        {
            _context.ChatSessions.Add(session);
            await _context.SaveChangesAsync();
            return session;
        }

        public async Task UpdateSessionAsync(ChatSession session)
        {
            session.UpdatedAt = DateTime.UtcNow;
            _context.ChatSessions.Update(session);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteSessionAsync(ChatSession session)
        {
            session.IsDeleted = true;
            session.UpdatedAt = DateTime.UtcNow;
            _context.ChatSessions.Update(session);
            await _context.SaveChangesAsync();
        }

        // ── Messages ─────────────────────────────────────────────────────────────

        public async Task<IEnumerable<ChatMessage>> GetMessagesBySessionAsync(int chatSessionId, int skip = 0, int take = 50)
        {
            return await _context.ChatMessages
                .Where(m => m.ChatSessionId == chatSessionId && !m.IsDeleted)
                .OrderBy(m => m.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<ChatMessage> AddMessageAsync(ChatMessage message)
        {
            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task UpdateMessageAsync(ChatMessage message)
        {
            message.UpdatedAt = DateTime.UtcNow;
            _context.ChatMessages.Update(message);
            await _context.SaveChangesAsync();
        }

        public async Task<ChatMessage?> GetMessageByIdAsync(int messageId)
        {
            return await _context.ChatMessages
                .Where(m => m.Id == messageId && !m.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task MarkSessionMessagesReadAsync(int chatSessionId, int userId)
        {
            var unread = await _context.ChatMessages
                .Where(m => m.ChatSessionId == chatSessionId
                         && m.MessageType == ChatMessageType.Bot
                         && !m.IsRead
                         && !m.IsDeleted)
                .ToListAsync();

            foreach (var msg in unread)
            {
                msg.IsRead = true;
                msg.UpdatedAt = DateTime.UtcNow;
            }

            if (unread.Count > 0)
                await _context.SaveChangesAsync();
        }

        public async Task<int> GetUnreadCountAsync(int chatSessionId)
        {
            return await _context.ChatMessages
                .CountAsync(m => m.ChatSessionId == chatSessionId
                              && m.MessageType == ChatMessageType.Bot
                              && !m.IsRead
                              && !m.IsDeleted);
        }
    }
}
