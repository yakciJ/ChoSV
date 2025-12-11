namespace ChoSV.Models.DTOs.UserWallPost
{
    public class UserWallPostListDTO
    {
        public int UserWallPostId { get; set; }
        public required string UserWallOwnerId { get; set; }
        public required string PosterId { get; set; }
        public string? PosterAvatarImage { get; set; }
        public required string PosterUserName { get; set; }
        public string? PosterFullName { get; set; }
        public required string CommentContent { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
