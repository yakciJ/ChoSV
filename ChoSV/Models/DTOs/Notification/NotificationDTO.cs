using System.ComponentModel.DataAnnotations;

namespace ChoSV.Models.DTOs.Notification
{
    public class NotificationDTO
    {
        [Required]
        public required int NotificationId { get; set; }
        [Required]
        public required string UserId { get; set; }
        [Required]
        public required string Message { get; set; }
        public bool IsRead { get; set; }
        public int? ProductId { get; set; }
        public int? UserWallPostId { get; set; }
        public string? FromUserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}