using ChoSV.Models.DTOs.Chat;
using ChoSV.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace ChoSV.Hubs
{
    [Authorize(Policy = "UserPolicy")]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private static readonly ConcurrentDictionary<string, string> ConnectedUsers = new();

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                ConnectedUsers.AddOrUpdate(userId, Context.ConnectionId, (key, oldValue) => Context.ConnectionId);
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");

                // Notify others that user is online
                await Clients.All.SendAsync("UserOnline", userId);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                ConnectedUsers.TryRemove(userId, out _);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");

                // Notify others that user is offline
                await Clients.All.SendAsync("UserOffline", userId);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(SendMessageDTO sendMessageDTO)
        {
            var senderId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (senderId == null)
            {
                await Clients.Caller.SendAsync("Error", "Chưa đăng nhập!");
                return;
            }

            try
            {
                var message = await _chatService.SendMessageAsync(senderId, sendMessageDTO);

                // Send to receiver if online
                if (ConnectedUsers.TryGetValue(sendMessageDTO.ReceiverId, out var receiverConnectionId))
                {
                    await Clients.Client(receiverConnectionId).SendAsync("ReceiveMessage", message);
                }

                // Send confirmation to sender
                await Clients.Caller.SendAsync("MessageSent", message);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        public async Task MarkAsRead(int messageId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return;

            try
            {
                await _chatService.MarkAsReadAsync(userId, messageId);
                await Clients.Caller.SendAsync("MessageMarkedAsRead", messageId);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        public async Task JoinPrivateChat(string receiverId)
        {
            var senderId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (senderId == null) return;

            var chatRoomName = GetChatRoomName(senderId, receiverId);
            await Groups.AddToGroupAsync(Context.ConnectionId, chatRoomName);
        }

        public async Task LeavePrivateChat(string receiverId)
        {
            var senderId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (senderId == null) return;

            var chatRoomName = GetChatRoomName(senderId, receiverId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatRoomName);
        }

        public async Task GetOnlineUsers()
        {
            try
            {
                // Add safety check and limit the response size
                var userCount = ConnectedUsers.Count;
                if (userCount > 10000) // Arbitrary safety limit
                {
                    await Clients.Caller.SendAsync("Error", "Too many connected users to retrieve");
                    return;
                }

                var onlineUserIds = ConnectedUsers.Keys.Take(1000).ToList(); // Limit to first 1000 users
                await Clients.Caller.SendAsync("OnlineUsers", onlineUserIds);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", $"Failed to retrieve online users: {ex.Message}");
            }
        }

        private static string GetChatRoomName(string userId1, string userId2)
        {
            var users = new[] { userId1, userId2 }.OrderBy(x => x).ToArray();
            return $"chat_{users[0]}_{users[1]}";
        }
    }
}
