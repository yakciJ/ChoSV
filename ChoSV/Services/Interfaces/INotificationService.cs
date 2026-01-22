using ChoSV.Models.DTOs.Common;
using ChoSV.Models.DTOs.Notification;
using ChoSV.Models.Entities;

namespace ChoSV.Services.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationDTO> SendNotificationAsync(SendNotificationDTO sendNotificationDTO);
        Task<List<NotificationDTO>> SendNotificationToAllUserAsync(string message);
        Task<PagedResult<NotificationDTO>> GetNotificationHistoryAsync(string userId, int page = 1, int pageSize = 10);
        Task MarkAsReadAsync(string userId, int notificationId);
        Task DeleteNotificationAsync(string userId, int notificationId);

        Task<int> GetUnreadNotificationCountAsync(string userId);
        Task SendProductNotificationAsync(Product product);

        //Task SendProductNotificationAsync(string userId, string productName);
        //Task SendUserWallPostNotificationAsync(UserWallPost userWallPost); // nguoi dung abc vua viet len tuong cua ban
        //Task UpdateNotificationAsync(int notificationId, string newMessage);
        //Task SendMessageNotificationAsync(string receiverId, string senderId);

        //Task SendReportNotificationAsync(); // cai nay khi nao bi admin xu ly moi tinh nhi? khi đổi trạng thái của report từ pending sang Approved thì mới gửi thông báo cho cả 2, còn Rejected thì bỏ qua.
        // thong bao cho nguoi đã report nếu report đúng, kiểu cảm ơn bạn đã report, chúng tôi đã xử lý,...
        // thông báo cho người bị report nếu report đúng, Bài viết, bình luận, thông tin cá nhân của bạn đã vi phạm quy định của nền tảng.
    }
}
