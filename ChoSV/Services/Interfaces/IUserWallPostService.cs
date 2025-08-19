using ChoSV.Models.DTOs.Common;
using ChoSV.Models.DTOs.UserWallPost;

namespace ChoSV.Services.Interfaces
{
    public interface IUserWallPostService
    {
        Task<PagedResult<UserWallPostListDTO>> GetUserWallPostById(string userId, int page = 1, int pageSize = 10);
        Task CreateUserWallPostAsync(string userId, CreateUserWallPostDTO createUserWallPostDTO);
        Task UpdateUserWallPostAsync(string userId, UpdateUserWallPostDTO updateUserWallPostDTO);
        Task DeleteUserWallPostByIdAsync(string userId, int userWallPostId);
    }
}
