using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChoSV.Models.Entities
{
    public class Favorite
    {
        [Required]
        public required string UserId { get; set; }
        public int ProductId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [ForeignKey("UserId")]
        [InverseProperty("Favorites")]
        public virtual User User { get; set; } = null!;
        [ForeignKey("ProductId")]
        [InverseProperty("Favorites")]
        public virtual Product Product { get; set; } = null!;
    }
}
