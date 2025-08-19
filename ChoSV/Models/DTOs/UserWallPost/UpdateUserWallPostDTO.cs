namespace ChoSV.Models.DTOs.UserWallPost
{
    public class UpdateUserWallPostDTO
    {
        public int UserWallPostId { get; set; }
        public required string CommentContent { get; set; }
    }
}
