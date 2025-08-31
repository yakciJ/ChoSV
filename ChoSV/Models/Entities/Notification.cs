using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChoSV.Models.Entities
{
    public class Notification
    {
        public int NotificationId { get; set; }
        [Required]
        public required string UserId { get; set; }
        [Required]
        public required string Message { get; set; }
        public bool IsRead { get; set; } = false;
        public int? ProductId { get; set; } // sản phẩm của bạn đã được duyệt/hủy bỏ.
        public int? UserWallPostId { get; set; } // có bình luận mới trên tường
        public string? FromUserId { get; set; } // thông báo người dùng này comment lên tường của bạn với userWallPostId này, (hoặc người dùng này muốn/đã đặt mua sản phẩm của bạn?) 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [ForeignKey("UserId")]
        [InverseProperty("Notifications")]
        public virtual User? User { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
        [ForeignKey("FromUserId")]
        public virtual User? FromUser { get; set; }
        [ForeignKey("UserWallPostId")]
        public virtual UserWallPost? UserWallPost { get; set; }
    }
}
