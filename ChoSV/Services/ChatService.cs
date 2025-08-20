using ChoSV.Data;
using ChoSV.Models.DTOs.Chat;
using ChoSV.Models.DTOs.Common;
using ChoSV.Models.Mappers;
using ChoSV.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChoSV.Services
{
    public class ChatService : IChatService
    {
        private readonly ApplicationDBContext _dbContext;
        public ChatService(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<MessageDTO> SendMessageAsync(string senderId, SendMessageDTO sendMessageDTO)
        {
            var receiver = await _dbContext.Users
                .FindAsync(sendMessageDTO.ReceiverId);
            if (receiver == null)
            {
                throw new ArgumentException("Người nhận không tồn tại!");
            }

            var sender = await _dbContext.Users.FindAsync(senderId);
            if (sender == null)
            {
                throw new ArgumentException("Người gửi không tồn tại!");
            }

            var message = sendMessageDTO.ToMessageFromDTO(senderId);

            _dbContext.Messages.Add(message);
            await _dbContext.SaveChangesAsync();

            return message.ToMessageDTO(sender.UserName!);
        }

        public async Task<PagedResult<MessageDTO>> GetChatHistoryAsync(string userId, string otherUserId, int page = 1, int pageSize = 10)
        {
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("Người dùng không tồn tại!");
            }
            var otherUser = await _dbContext.Users.FindAsync(otherUserId);
            if (otherUser == null)
            {
                throw new ArgumentException("Người dùng không tồn tại!");
            }

            var query = _dbContext.Messages
                .Include(m => m.Sender)
                .Where(m => (m.SenderId == userId && m.ReceiverId == otherUserId) || (m.SenderId == otherUserId && m.ReceiverId == userId))
                .OrderByDescending(m => m.CreatedDate);

            var totalCount = await query.CountAsync();
            var messages = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => m.ToMessageDTO(m.Sender != null ? m.Sender.UserName! : string.Empty))
                .ToListAsync();

            return new PagedResult<MessageDTO>
            {
                Items = messages.OrderBy(m => m.CreatedDate).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task MarkAsReadAsync(string userId, int messageId)
        {
            var message = await _dbContext.Messages.FindAsync(messageId);
            if (message == null)
            {
                throw new ArgumentException("Tin nhắn không tồn tại!");
            }

            message.IsRead = true;
            await _dbContext.SaveChangesAsync();
        }

        public async Task<MessageDTO?> GetNewestUnreadMessageAsync(string userId)
        {
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("Người dùng không tồn tại!");
            }

            var message = await _dbContext.Messages
                .Include(m => m.Sender)
                .Where(m => m.ReceiverId == userId && !m.IsRead)
                .OrderByDescending(m => m.CreatedDate)
                .Select(m => m.ToMessageDTO(m.Sender!.UserName!))
                .FirstOrDefaultAsync();

            return message;
        }

        public async Task<int> GetUnreadMessagesCountAsync(string userId)
        {
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("Người dùng không tồn tại!");
            }

            return await _dbContext.Messages
                .CountAsync(m => m.ReceiverId == userId && !m.IsRead);
        }

        public async Task<List<MessageDTO>> GetRecentChatsAsync(string userId)
        {
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("Người dùng không tồn tại!");
            }

            var recentMessages = await _dbContext.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .Select(g => g.OrderByDescending(m => m.CreatedDate).First())
                .OrderByDescending(m => m.CreatedDate)
                .Select(m => m.ToMessageDTO(m.Sender!.UserName!))
                .ToListAsync();

            return recentMessages;
        }
    }
}
