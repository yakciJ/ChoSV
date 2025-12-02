using ChoSV.Models.DTOs.Notification;
using ChoSV.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChoSV.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "UserPolicy")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }
        // 3 cai service con lai trong hub.
        [HttpGet]
        public async Task<IActionResult> GetNotificationHistoryAsync()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }
            return Ok(await _notificationService.GetNotificationHistoryAsync(userId));
        }

        [HttpGet("unreadCount")]
        public async Task<IActionResult> GetUnreadNotificationCountAsync()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }
            return Ok(await _notificationService.GetUnreadNotificationCountAsync(userId));
        }

        [HttpPut("{notificationId}/read")]
        [Authorize(Policy = "UserPolicy")]
        public async Task<IActionResult> MarkNotificationAsReadAsync(int notificationId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }
            await _notificationService.MarkAsReadAsync(userId, notificationId);
            return Ok(new { message = "Đã đánh dấu thông báo là đã đọc!" });
        }

        [HttpDelete("{notificationId}")]
        [Authorize(Policy = "UserPolicy")]
        public async Task<IActionResult> DeleteNotificationAsync(int notificationId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }
            // Assuming there's a method to delete notification in the service
            await _notificationService.DeleteNotificationAsync(userId, notificationId);
            return Ok(new { message = "Đã xóa thông báo!" });
        }

        [HttpPost]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> SendNotificationToUserAsync(SendNotificationDTO sendNotificationDTO)
        {
            await _notificationService.SendNotificationAsync(sendNotificationDTO);
            return Ok(new { message = "Gửi thông báo thành công!" });
        }

        [HttpPost("all")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> SendNotificationToAllUserAsync(string message)
        {
            await _notificationService.SendNotificationToAllUserAsync(message);
            return Ok(new { message = "Gửi thông báo thành công!" });
        }

        [HttpPut("admin/{notificationId}")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> UpdateNotificationAsync(int notificationId, string message)
        {
            await _notificationService.UpdateNotificationAsync(notificationId, message);
            return Ok(new { message = "Cập nhật thông báo thành công!" });
        }
    }
