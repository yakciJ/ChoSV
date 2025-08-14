using ChoSV.Models.DTOs.User;
using ChoSV.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace ChoSV.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService) => _userService = userService;
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            await _userService.RegisterAsync(registerDTO);
            return Ok(new { message = "Đăng ký thành công!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            return Ok(await _userService.LoginAsync(loginDTO));
        }

        [HttpPost("confirmEmail")]
        public async Task<IActionResult> ConfirmEmailAsync(string email, string token)
        {
            await _userService.ConfirmEmailAsync(email, token);
            return Ok(new { message = "Xác thực email thành công!" });
        }

        [HttpPut("profile")]
        [Authorize(Policy = "UserPolicy")]
        public async Task<IActionResult> UpdateProfileAsync(UpdateUserDTO updateUserDTO)
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.NameId)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Không thể xác định người dùng!");
            }
            await _userService.UpdateUserProfileAsync(userId, updateUserDTO);
            return Ok(new { message = "Cập nhập thông tin thành công!" });
        }

        [HttpDelete("account")]
        [Authorize(Policy = "UserPolicy")]
        public async Task<IActionResult> DeleteUserAsync()
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.NameId)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Không thể xác định người dùng!");
            }
            await _userService.DeleteUserAsync(userId);
            return Ok(new { message = "Xóa tài khoản thành công!" });
        }

        [HttpDelete("admin/{userId}")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> AdminDeleteUserAsync(string userId)
        {
            await _userService.DeleteUserAsync(userId);
            return Ok(new { message = "Xóa tài khoản thành công!" });
        }

        [HttpPut("changePassword")]
        [Authorize(Policy = "UserPolicy")]
        public async Task<IActionResult> ChangePasswordAsync(ChangePasswordDTO changePasswordDTO)
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.NameId)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Không thể xác định người dùng!");
            }
            await _userService.ChangePasswordAsync(userId, changePasswordDTO);
            return Ok(new { message = "Cập nhập mật khẩu thành công!" });
        }

        [HttpPost("forgotPassword")]
        public async Task<IActionResult> ForgotPasswordAsync(string email)
        {
            await _userService.ForgotPasswordAsync(email);
            return Ok(new { message = "Nếu email tồn tại, link đặt lại mật khẩu đã được gửi!" });
        }

        [HttpPut("resetPassword")]
        public async Task<IActionResult> ResetPasswordAsync(ResetPasswordDTO resetPasswordDTO)
        {
            await _userService.ResetPasswordAsync(resetPasswordDTO);
            return Ok(new { message = "Mật khẩu đã được đổi thành công!" });
        }
    }
}
