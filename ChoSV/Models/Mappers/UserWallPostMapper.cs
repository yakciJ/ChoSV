using ChoSV.Models.DTOs.UserWallPost;
using ChoSV.Models.Entities;

namespace ChoSV.Models.Mappers
{
    public static class UserWallPostMapper
    {
        public static UserWallPostListDTO ToUserWallPostListDTO(this UserWallPost userWallPost)
        {
            return new UserWallPostListDTO
            {
                UserWallPostId = userWallPost.UserWallPostId,
                UserWallOwnerId = userWallPost.UserWallOwnerId,
                PosterId = userWallPost.PosterId,
                PosterAvatarImage = userWallPost.Poster?.AvatarImage,
                PosterUserName = userWallPost.Poster?.UserName ?? string.Empty,
                CommentContent = userWallPost.CommentContent,
                CreatedAt = userWallPost.CreatedAt
            };
        }

        public static UserWallPostDetailListDTO ToUserWallPostDetailListDTO(this UserWallPost userWallPost)
        {
            return new UserWallPostDetailListDTO
            {
                UserWallPostId = userWallPost.UserWallPostId,
                UserWallOwnerId = userWallPost.UserWallOwnerId,
                PosterId = userWallPost.PosterId,
                PosterUserName = userWallPost.Poster?.UserName ?? string.Empty,
                CommentContent = userWallPost.CommentContent,
                CreatedAt = userWallPost.CreatedAt
            };
        }
    }
}
