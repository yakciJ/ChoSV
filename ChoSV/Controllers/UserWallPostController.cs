using ChoSV.Models.DTOs.UserWallPost;
using ChoSV.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace ChoSV.Controllers
{
    [Route("api/{controller}")]
    [ApiController]
    public class UserWallPostController : ControllerBase
    {
        private readonly IUserWallPostService _userWallPostService;
        public UserWallPostController(IUserWallPostService userWallPostService)
        {
            _userWallPostService = userWallPostService;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserWallPostById(string userId, int page = 1, int pageSize = 10)
        {
            return Ok(await _userWallPostService.GetUserWallPostById(userId, page, pageSize));
        }

        [HttpPost]
        [Authorize(Policy = "UserPolicy")]
        public async Task<IActionResult> CreateUserWallPostAsync(CreateUserWallPostDTO createUserWallPostDTO)
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.NameId)?.Value;
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
            var userId = User.FindFirst(JwtRegisteredClaimNames.NameId)?.Value;
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
            var userId = User.FindFirst(JwtRegisteredClaimNames.NameId)?.Value;
            if (userId == null)
            {
                return Unauthorized("Chưa đăng nhập!");
            }
            await _userWallPostService.DeleteUserWallPostByIdAsync(userId, userWallPostId);
            return Ok("Xóa bình luận thành công!");
        }
    }
}
