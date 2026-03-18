using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace WebApi.Hubs
{
    /// <summary>
    /// SignalR hub for real-time chat features: typing indicators, live message delivery.
    /// Clients join a group per session: "session-{sessionId}".
    /// </summary>
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(ILogger<ChatHub> logger)
        {
            _logger = logger;
        }

        private int GetUserId() =>
            int.Parse(Context.User!.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException("User ID claim missing in SignalR context"));

        /// <summary>Join the group for a specific chat session to receive live updates</summary>
        public async Task JoinSession(string sessionId)
        {
            var groupName = $"session-{sessionId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _logger.LogDebug("Connection {ConnectionId} joined group {Group}", Context.ConnectionId, groupName);
        }

        /// <summary>Leave a session group</summary>
        public async Task LeaveSession(string sessionId)
        {
            var groupName = $"session-{sessionId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        /// <summary>Broadcast typing indicator to other members of the session group</summary>
        public async Task SendTypingIndicator(string sessionId, bool isTyping)
        {
            var groupName = $"session-{sessionId}";
            await Clients.OthersInGroup(groupName).SendAsync("TypingIndicator", new
            {
                sessionId,
                isTyping,
                timestamp = DateTime.UtcNow
            });
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                var userId = GetUserId();
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
                _logger.LogDebug("ChatHub connected: user={UserId} connection={ConnectionId}", userId, Context.ConnectionId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "ChatHub connection failed for {ConnectionId}", Context.ConnectionId);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogDebug("ChatHub disconnected: {ConnectionId}", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }

    /// <summary>
    /// Helper service to push real-time events to specific sessions from outside the hub.
    /// </summary>
    public interface IChatHubNotifier
    {
        Task NotifyNewMessage(string sessionId, ChatMessageDto message);
        Task NotifyTyping(string sessionId, bool isTyping);
        Task NotifySessionUpdated(string sessionId, ChatSessionDto session);
    }

    public class ChatHubNotifier : IChatHubNotifier
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatHubNotifier(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public Task NotifyNewMessage(string sessionId, ChatMessageDto message) =>
            _hubContext.Clients.Group($"session-{sessionId}")
                .SendAsync("ReceiveMessage", message);

        public Task NotifyTyping(string sessionId, bool isTyping) =>
            _hubContext.Clients.Group($"session-{sessionId}")
                .SendAsync("TypingIndicator", new { sessionId, isTyping, timestamp = DateTime.UtcNow });

        public Task NotifySessionUpdated(string sessionId, ChatSessionDto session) =>
            _hubContext.Clients.Group($"session-{sessionId}")
                .SendAsync("SessionUpdated", session);
    }
}
