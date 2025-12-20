using ChoSV.Data;
using ChoSV.Models.DTOs.Common;
using ChoSV.Models.DTOs.UserWallPost;
using ChoSV.Models.Entities;
using ChoSV.Models.Mappers;
using ChoSV.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ChoSV.Services
{
    public class UserWallPostService : IUserWallPostService
    {
        private readonly ApplicationDBContext _dbcontext;
        private readonly UserManager<User> _userManager;
        public UserWallPostService(ApplicationDBContext dbcontext, UserManager<User> userManager)
        {
            _dbcontext = dbcontext;
            _userManager = userManager;
        }

        public async Task<PagedResult<UserWallPostListDTO>> GetUserWallPostByUserName(string userName, int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var user = await _dbcontext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.UserName == userName);

            if (user == null)
            {
                throw new ArgumentException("Nguời dùng không tồn tại");
            }

            var query = _dbcontext.UserWallPosts
                .AsNoTracking()
                .Include(u => u.Poster)
                .Where(u => u.UserWallOwnerId == user.Id)
                .OrderByDescending(u => u.CreatedAt);

            var totalCount = await query.CountAsync();

            var userWallPosts = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var userWallPostDTOs = userWallPosts.Select(q => q.ToUserWallPostListDTO()).ToList();

            return new PagedResult<UserWallPostListDTO>
            {
                Items = userWallPostDTOs,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task CreateUserWallPostAsync(string userId, CreateUserWallPostDTO createUserWallPostDTO)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("Người dùng không tồn tại!");
            }
            var wallOwner = await _userManager
                .FindByIdAsync(createUserWallPostDTO.UserWallOwnerId);
            if (wallOwner == null)
            {
                throw new ArgumentException("Người dùng không tồn tại!");
            }

            var userWallPost = new UserWallPost
            {
                UserWallOwnerId = createUserWallPostDTO.UserWallOwnerId,
                CommentContent = createUserWallPostDTO.CommentContent,
                PosterId = userId,
            };
            _dbcontext.UserWallPosts.Add(userWallPost);
            await _dbcontext.SaveChangesAsync();
        }

        public async Task UpdateUserWallPostAsync(string userId, UpdateUserWallPostDTO updateUserWallPostDTO)
        {
            var userWallPost = await _dbcontext.UserWallPosts.FirstOrDefaultAsync(u => u.UserWallPostId == updateUserWallPostDTO.UserWallPostId);
            if (userWallPost == null)
            {
                throw new ArgumentException("Không tìm thấy bình luận!");
            }
            if (userWallPost.PosterId != userId)
            {
                throw new ArgumentException("Không chỉnh sửa được bình luận của người khác!");
            }
            userWallPost.CommentContent = updateUserWallPostDTO.CommentContent;
            await _dbcontext.SaveChangesAsync();
        }

        public async Task DeleteUserWallPostByIdAsync(string userId, int userWallPostId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("Không tìm thấy người dùng!");
            }
            var userWallPost = await _dbcontext.UserWallPosts.FirstOrDefaultAsync(u => u.UserWallPostId == userWallPostId);

            if (userWallPost == null)
            {
                throw new ArgumentException("Không tìm thấy bình luận!");
            }

            if (userWallPost.PosterId != userId)
            {
                throw new ArgumentException("Không xóa được bình luận của người khác!");
            }

            _dbcontext.UserWallPosts.Remove(userWallPost);
            await _dbcontext.SaveChangesAsync();
        }

        public async Task<PagedResult<UserWallPostDetailListDTO>> GetAllUserWallPostsAsync(int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var query = _dbcontext.UserWallPosts
                .AsNoTracking()
                .Include(u => u.Poster)
                .OrderByDescending(u => u.CreatedAt);

            var totalCount = await query.CountAsync();

            var userWallPosts = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var userWallPostDTOs = userWallPosts.Select(q => q.ToUserWallPostDetailListDTO()).ToList();

            return new PagedResult<UserWallPostDetailListDTO>
            {
                Items = userWallPostDTOs,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task AdminDeleteUserWallPostByIdAsync(int userWallPostId)
        {
            var userWallPost = await _dbcontext.UserWallPosts.FirstOrDefaultAsync(u => u.UserWallPostId == userWallPostId);

            if (userWallPost == null)
            {
                throw new ArgumentException("Không tìm thấy bình luận!");
            }

            _dbcontext.UserWallPosts.Remove(userWallPost);
            await _dbcontext.SaveChangesAsync();
        }
    }
}
