using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChoSV.Models.Entities
{
    public class UserViewHistory
    {
        [Required]
        public string UserId { get; set; } = null!;

        [Required]
        public int ProductId { get; set; }

        public int ViewCount { get; set; } = 1;
        public DateTime LastViewedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        [InverseProperty("ViewHistories")]
        public virtual User Viewer { get; set; } = null!;

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;
    }
}
