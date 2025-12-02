using ChoSV.Data;
using ChoSV.Hubs;
using ChoSV.Models.DTOs.Common;
using ChoSV.Models.DTOs.Notification;
using ChoSV.Models.Entities;
using ChoSV.Models.Mappers;
using ChoSV.Services.Interfaces;
using EFCore.BulkExtensions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChoSV.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDBContext _dbContext;
        private readonly IHubContext<NotificationHub> _hubContext;
        public NotificationService(IHubContext<NotificationHub> hubContext, ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
            _hubContext = hubContext;
        }

        public async Task<NotificationDTO> SendNotificationAsync(SendNotificationDTO sendNotificationDTO)
        {
            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == sendNotificationDTO.UserId);
            if (user == null)
            {
                throw new ArgumentException("Người dùng không tồn tại!");
            }
            if (sendNotificationDTO.Message == null)
            {
                throw new ArgumentException("Thông báo không được để trống!");
            }
            var noti = sendNotificationDTO.ToNotificationFromSendNotificationDTO();
            _dbContext.Notifications.Add(noti);
            await _dbContext.SaveChangesAsync();

            await _hubContext.Clients.User(sendNotificationDTO.UserId)
                .SendAsync("ReceiveNotification", noti.ToNotificationDTOFromNotification());

            return noti.ToNotificationDTOFromNotification();
        }

        public async Task<List<NotificationDTO>> SendNotificationToAllUserAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("Thông báo không được để trống!");
            }
            var userIds = await _dbContext.Users
                .AsNoTracking()
                .Where(u => !u.IsBanned)
                .Select(u => u.Id)
                .ToListAsync();
            var notifications = userIds
                .Select(userId => NotificationMapper.ToNotificationListFromSendNotificationDTO(userId, message))
                .ToList();

            await _dbContext.BulkInsertAsync(notifications);

            var notificationDTOs = notifications
                .Select(n => n.ToNotificationDTOFromNotification())
                .ToList();

            var tasks = notificationDTOs.Select(noti => _hubContext.Clients.User(noti.UserId).SendAsync("ReceiveNotification", noti));
            await Task.WhenAll(tasks);

            return notificationDTOs;
        }

        public async Task<PagedResult<NotificationDTO>> GetNotificationHistoryAsync(string userId, int page = 1, int pageSize = 10)
        {
            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new ArgumentException("Người dùng không tồn tại!");
            }
            var query = _dbContext.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId);

            var totalCount = await query.CountAsync();

            var notifications = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var notiDTOS = notifications.Select(n => n.ToNotificationDTOFromNotification()).ToList();

            return new PagedResult<NotificationDTO>
            {
                Items = notiDTOS,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task MarkAsReadAsync(string userId, int notificationId)
        {
            var user = _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new ArgumentException("Người dùng không tồn tại!");
            }
            var noti = await _dbContext.Notifications
                .Where(n => n.UserId == userId
                        && !n.IsRead
                        && n.NotificationId == notificationId)
                .FirstOrDefaultAsync();
            if (noti == null)
            {
                throw new ArgumentException("Thông báo không tồn tại!");
            }

            noti.IsRead = true;
            await _dbContext.SaveChangesAsync();
        }

        public async Task<int> GetUnreadNotificationCountAsync(string userId)
        {
            var totalCount = await _dbContext.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId)
                .CountAsync();
            return totalCount;
        }

        public async Task DeleteNotificationAsync(string userId, int notificationId)
        {
            //var user = await _dbContext.Users
            //    .AsNoTracking()
            //    .FirstOrDefaultAsync(u => u.Id == userId);
            //if (user == null)
            //{
            //    throw new ArgumentException("Người dùng không tồn tại!");
            //}
            var noti = await _dbContext.Notifications
                .Where(n => n.UserId == userId
                        && n.NotificationId == notificationId)
                .FirstOrDefaultAsync();
            if (noti == null)
            {
                throw new ArgumentException("Thông báo không tồn tại!");
            }
            _dbContext.Notifications.Remove(noti);
            await _dbContext.SaveChangesAsync();
        }


        public async Task SendProductNotificationAsync(Product product)
        {
            var message = "";
            if (product.Status == "Approved")
            {
                message = $"Sản phẩm {product.ProductName} của bạn đã được duyệt!";
            }
            else if (product.Status == "Rejected")
            {
                message = $"Sản phẩm {product.ProductName} của bạn đã bị từ chối!";

            }
            var noti = new SendNotificationDTO
            {
                UserId = product.SellerId,
                Message = message,
                ProductId = product.ProductId
            };
            await SendNotificationAsync(noti);
        }
        public async Task SendProductNotificationAsync(string userId, string productName)
        {
            var message = $"Sản phẩm {productName} của bạn đã bị xóa!";
            var noti = new SendNotificationDTO
            {
                UserId = userId,
                Message = message,
            };
            await SendNotificationAsync(noti);
        }

        public async Task SendUserWallPostNotificationAsync(UserWallPost userWallPost)
        {
            var message = $"Người dùng {userWallPost.Owner.UserName} vừa bình luận lên tường của bạn";
            var noti = new SendNotificationDTO
            {
                UserId = userWallPost.UserWallOwnerId,
                Message = message,
                UserWallPostId = userWallPost.UserWallPostId,
                FromUserId = userWallPost.PosterId
            };
            await SendNotificationAsync(noti);
        }

        public async Task UpdateNotificationAsync(int notificationId, string newMessage)
        {
            var noti = await _dbContext.Notifications
                .Where(n => n.NotificationId == notificationId)
                .FirstOrDefaultAsync();
            if (noti == null)
            {
                throw new ArgumentException("Thông báo không tồn tại!");
            }
            noti.Message = newMessage;
            await _dbContext.SaveChangesAsync();
        }

        //public async Task SendMessageNotificationAsync(string receiverId, string senderName)
        //{
        //    var message = $"Bạn có một tin nhắn mới từ {senderName}";
        //    var noti = new SendNotificationDTO
        //    {
        //        UserId = receiverId,
        //        Message = message,
        //    };
        //    await SendNotificationAsync(noti);
        //}

    }
}
