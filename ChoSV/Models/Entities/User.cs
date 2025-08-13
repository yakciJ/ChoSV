using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChoSV.Models.Entities
{
    [Table("User")]
    public class User : IdentityUser
    {
        [StringLength(100)]
        public string? FullName { get; set; }
        [StringLength(255)]
        public string? AvatarImage { get; set; }
        public string? Bio { get; set; }
        public bool IsBanned { get; set; } = false;
        public int WarningCount { get; set; } = 0;
        public DateTime LastWarning { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [InverseProperty("User")]
        public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        [InverseProperty("Sender")]
        public virtual ICollection<Message> SentMessages { get; set; } = new List<Message>();
        [InverseProperty("Receiver")]
        public virtual ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
        [InverseProperty("Owner")]
        public virtual ICollection<UserWallPost> WallPosts { get; set; } = new List<UserWallPost>();
        [InverseProperty("Poster")]
        public virtual ICollection<UserWallPost> PostsMade { get; set; } = new List<UserWallPost>();
        [InverseProperty("User")]
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        [InverseProperty("Seller")]
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        [InverseProperty("Reporter")]
        public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
        [InverseProperty("User")]
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
