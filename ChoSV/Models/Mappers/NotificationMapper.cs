using ChoSV.Models.DTOs.Notification;
using ChoSV.Models.Entities;

namespace ChoSV.Models.Mappers
{
    public static class NotificationMapper
    {
        public static Notification ToNotificationFromSendNotificationDTO(this SendNotificationDTO sendNotificationDTO)
        {
            return new Notification
            {
                UserId = sendNotificationDTO.UserId,
                Message = sendNotificationDTO.Message,
                ProductId = sendNotificationDTO.ProductId,
                UserWallPostId = sendNotificationDTO.UserWallPostId,
                FromUserId = sendNotificationDTO.UserId
            };
        }
        public static Notification ToNotificationListFromSendNotificationDTO(string userId, string message)
        {
            return new Notification
            {
                UserId = userId,
                Message = message,
            };
        }

        public static NotificationDTO ToNotificationDTOFromNotification(this Notification notification)
        {
            return new NotificationDTO
            {
                NotificationId = notification.NotificationId,
                UserId = notification.UserId,
                Message = notification.Message,
                IsRead = notification.IsRead,
                ProductId = notification.ProductId,
                UserWallPostId = notification.UserWallPostId,
                FromUserId = notification.FromUserId,
                CreatedAt = notification.CreatedAt
            };
        }
    }
}
