using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChoSV.Models.Entities
{
    public class UserWallPost
    {
        public int UserWallPostId { get; set; }
        [Required]
        public required string UserWallOwnerId { get; set; }
        [Required]
        public required string PosterId { get; set; }
        [Required]
        public required string CommentContent { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserWallOwnerId")]
        [InverseProperty("WallPosts")]
        public virtual User Owner { get; set; } = null!;
        [ForeignKey("PosterId")]
        [InverseProperty("PostsMade")]
        public virtual User Poster { get; set; } = null!;

    }
}
