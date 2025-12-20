using ChoSV.Models.DTOs.UserWallPost;
using ChoSV.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChoSV.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserWallPostController : ControllerBase
    {
        private readonly IUserWallPostService _userWallPostService;
        public UserWallPostController(IUserWallPostService userWallPostService)
        {
            _userWallPostService = userWallPostService;
        }

        [HttpGet("{userName}")]
        public async Task<IActionResult> GetUserWallPostByUserName(string userName, int page = 1, int pageSize = 10)
        {
            return Ok(await _userWallPostService.GetUserWallPostByUserName(userName, page, pageSize));
        }

        [HttpPost]
        [Authorize(Policy = "UserPolicy")]
        public async Task<IActionResult> CreateUserWallPostAsync(CreateUserWallPostDTO createUserWallPostDTO)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Chưa đăng nhập!");
            }
            await _userWallPostService.CreateUserWallPostAsync(userId, createUserWallPostDTO);
            return Ok(new { message = "Tạo bình luận thành công!" });
        }

        [HttpPut]
        [Authorize(Policy = "UserPolicy")]
        public async Task<IActionResult> UpdateUserWallPostAsync(UpdateUserWallPostDTO updateUserWallPostDTO)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Chưa đăng nhập!");
            }
            await _userWallPostService.UpdateUserWallPostAsync(userId, updateUserWallPostDTO);
            return Ok(new { message = "Chỉnh sửa bình luận thành công!" });
        }


        [HttpDelete("{userWallPostId}")]
        [Authorize(Policy = "UserPolicy")]
        public async Task<IActionResult> DeleteUserWallPostByIdAsync(int userWallPostId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Chưa đăng nhập!");
            }
            await _userWallPostService.DeleteUserWallPostByIdAsync(userId, userWallPostId);
            return Ok("Xóa bình luận thành công!");
        }

        [HttpGet]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> GetAllUserWallPostAsync(int page = 1, int pageSize = 10)
        {
            return Ok(await _userWallPostService.GetAllUserWallPostsAsync(page, pageSize));
        }

        [HttpDelete("admin/{userWallPostId}")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> AdminDeleteWallPostByIdAsync(int userWallPostId)
        {
            await _userWallPostService.AdminDeleteUserWallPostByIdAsync(userWallPostId);
            return Ok(new { message = "Xóa bình luận thành công!" });
        }
    }
}
