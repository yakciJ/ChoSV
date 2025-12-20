using ChoSV.Models.DTOs.Chat;
using ChoSV.Models.Entities;

namespace ChoSV.Models.Mappers
{
    public static class MessageMapper
    {
        public static Message ToMessageFromDTO(this SendMessageDTO sendMessageDTO, string senderId)
        {
            return new Message
            {
                SenderId = senderId,
                ReceiverId = sendMessageDTO.ReceiverId,
                Content = sendMessageDTO.Content,
                CreatedDate = DateTime.UtcNow,
                IsRead = false
            };
        }

        public static MessageDTO ToMessageDTO(this Message message, string senderUserName)
        {
            return new MessageDTO
            {
                MessageId = message.MessageId,
                SenderId = message.SenderId,
                SenderUserName = senderUserName,
                ReceiverId = message.ReceiverId,
                Content = message.Content,
                CreatedDate = message.CreatedDate,
                IsRead = message.IsRead,
            };
        }

        // New overload for recent chats with other user info
        public static MessageDTO ToRecentChatDTO(this Message message, string senderUserName, string currentUserId, User otherUser)
        {
            return new MessageDTO
            {
                MessageId = message.MessageId,
                SenderId = message.SenderId,
                SenderUserName = senderUserName,
                ReceiverId = message.ReceiverId,
                Content = message.Content,
                CreatedDate = message.CreatedDate,
                IsRead = message.IsRead,
                OtherUserName = otherUser.UserName,
                OtherUserId = otherUser.Id,
                OtherUserFullName = otherUser.FullName,
                OtherUserAvatar = otherUser.AvatarImage
            };
        }
    }
}
