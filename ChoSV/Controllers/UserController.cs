using ChoSV.Models.DTOs.User;
using ChoSV.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChoSV.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        public UserController(IUserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            await _userService.RegisterAsync(registerDTO);
            return Ok(new { message = "Đăng ký thành công!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            var res = await _userService.LoginAsync(loginDTO);
            var refreshTokenExpirationDays = loginDTO.RememberMe ? _configuration.GetValue<int>("JWT:RefreshTokenExpirationInDays") : _configuration.GetValue<int>("JWT:RefreshTokenShortExpirationInDays");
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                //Path = "/",
                //SameSite = SameSiteMode.Strict,
                Path = "/api/User/refreshToken",
                Expires = DateTime.UtcNow.AddDays(refreshTokenExpirationDays)
            };

            Response.Cookies.Append("refreshToken", res.RefreshToken, cookieOptions);

            res.RefreshToken = string.Empty;

            return Ok(res);
        }

        [HttpPost("resendConfirmEmail")]
        public async Task<IActionResult> SendConfirmEmail(string email)
        {
            await _userService.SendConfirmEmailAsync(email);
            return Ok(new { message = "Gửi lại email xác thực thành công!" });
        }

        [HttpPost("confirmEmail")]
        public async Task<IActionResult> ConfirmEmailAsync(string email, string token)
        {
            await _userService.ConfirmEmailAsync(email, token);
            return Ok(new { message = "Xác thực email thành công!" });
        }

        [HttpGet("refreshToken")]
        public async Task<IActionResult> GetAccessTokenAsync()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (refreshToken == null)
            {
                return Unauthorized("Không thể xác định người dùng!");
            }
            return Ok(await _userService.GetAccessTokenAsync(refreshToken));
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUserProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Không thể xác định người dùng!");
            }
            return Ok(await _userService.GetCurrentUserProfileAsync(userId));
        }

        [HttpGet("{userName}")]
        public async Task<IActionResult> GetUserByUserNameAsync(string userName)
        {
            return Ok(await _userService.GetUserByUserNameAsync(userName));
        }

        [HttpPut("profile")]
        [Authorize(Policy = "UserPolicy")]
        public async Task<IActionResult> UpdateUserProfileAsync(UpdateUserDTO updateUserDTO)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Không thể xác định người dùng!");
            }
            await _userService.UpdateUserProfileAsync(userId, updateUserDTO);
            return Ok(new { message = "Cập nhập thông tin thành công!" });
        }

        [HttpPut("avatar")]
        [Authorize(Policy = "UserPolicy")]
        public async Task<IActionResult> UpdateAvatarAsync(string imageUrl)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Không thể xác định người dùng!");
            }
            await _userService.UpdateAvatarAsync(userId, imageUrl);
            return Ok(new { message = "Đổi ảnh đại diện thành công!" });
        }

        [HttpDelete("account")]
        [Authorize(Policy = "UserPolicy")]
        public async Task<IActionResult> DeleteUserAsync()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Không thể xác định người dùng!");
            }
            await _userService.DeleteUserAsync(userId);
            return Ok(new { message = "Xóa tài khoản thành công!" });
        }

        [HttpPut("changePassword")]
        [Authorize(Policy = "UserPolicy")]
        public async Task<IActionResult> ChangePasswordAsync(ChangePasswordDTO changePasswordDTO)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
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

        [HttpGet("admin/users")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> GetAllUsersAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _userService.GetAllUsersByPageAsync(page, pageSize);
            return Ok(result);
        }

        [HttpDelete("admin/{userId}")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> AdminDeleteUserAsync(string userId)
        {
            await _userService.DeleteUserAsync(userId);
            return Ok(new { message = "Xóa tài khoản thành công!" });
        }

        [HttpPut("admin/ban/{userId}")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> AdminBanOrUnban(string userId)
        {
            await _userService.BanOrUnbanAsync(userId);
            return Ok(new { message = "Người dùng đã bị chặn/bỏ chặn thành công!" });
        }
    }
}
