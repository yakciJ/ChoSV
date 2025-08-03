using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChoSV.Models.Entities
{
    public class RefreshToken
    {
        public int RefreshTokenId { get; set; }
        [Required]
        [StringLength(500)]
        public required string Token { get; set; }
        [Required]
        public required string UserId { get; set; }
        public DateTime ExpiryDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRevoked { get; set; } = false;
        [StringLength(200)]
        public string? DeviceInfo { get; set; }
        [StringLength(45)]
        public string? IpAddress { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
        [NotMapped]
        public bool IsExpired => DateTime.UtcNow >= ExpiryDate;
        [NotMapped]
        public bool IsActive => !IsRevoked && !IsExpired;
    }
}
