using ChoSV.Models.DTOs.Common;
using ChoSV.Models.DTOs.UserWallPost;

namespace ChoSV.Services.Interfaces
{
    public interface IUserWallPostService
    {
        Task<PagedResult<UserWallPostListDTO>> GetUserWallPostByUserName(string userName, int page, int pageSize);
        Task CreateUserWallPostAsync(string userId, CreateUserWallPostDTO createUserWallPostDTO);
        Task UpdateUserWallPostAsync(string userId, UpdateUserWallPostDTO updateUserWallPostDTO);
        Task DeleteUserWallPostByIdAsync(string userId, int userWallPostId);

        // Còn api của Admin: Xem toàn bộ danh sách cmt, xóa cmt.

        Task<PagedResult<UserWallPostDetailListDTO>> GetAllUserWallPostsAsync(int page, int pageSize);
        Task AdminDeleteUserWallPostByIdAsync(int userWallPostId);
    }
}
