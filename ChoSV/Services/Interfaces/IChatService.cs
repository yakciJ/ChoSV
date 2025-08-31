using ChoSV.Models.DTOs.Chat;
using ChoSV.Models.DTOs.Common;

namespace ChoSV.Services.Interfaces
{
    public interface IChatService
    {
        Task<MessageDTO> SendMessageAsync(string senderId, SendMessageDTO messageDTO);
        Task<PagedResult<MessageDTO>> GetChatHistoryAsync(string userId, string otherUserId, int page = 1, int pageSize = 10);
        Task MarkAsReadAsync(string userId, int messageId);
        Task<MessageDTO?> GetNewestUnreadMessageAsync(string userId);
        Task<int> GetUnreadMessagesCountAsync(string userId);
        Task<List<MessageDTO>> GetRecentChatsAsync(string userId);
        // 2 cái GetRecentChatsAsync và GetNewestUnreadMessageAsync hình như hơi giống nhau
    }
}
