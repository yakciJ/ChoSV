using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChoSV.Models.Entities
{
    public class Notification
    {
        public int NotificationId { get; set; }
        public string? UserId { get; set; }
        [Required]
        public required string Message { get; set; }
        public bool IsRead { get; set; } = false;
        public int? ProductId { get; set; }
        public int? UserWallPostId { get; set; }
        public string? FromUserId { get; set; }
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
