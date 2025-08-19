namespace ChoSV.Models.DTOs.UserWallPost
{
    public class UserWallPostDetailListDTO
    {
        public int UserWallPostId { get; set; }
        public required string UserWallOwnerId { get; set; }
        public required string PosterId { get; set; }
        public required string PosterUserName { get; set; }
        public required string CommentContent { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
