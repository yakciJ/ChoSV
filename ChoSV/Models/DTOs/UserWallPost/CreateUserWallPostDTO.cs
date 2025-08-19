using System.ComponentModel.DataAnnotations;

namespace ChoSV.Models.DTOs.UserWallPost
{
    public class CreateUserWallPostDTO
    {
        [Required]
        public required string UserWallOwnerId { get; set; }
        [Required]
        public required string CommentContent { get; set; }
    }
}
