using ChoSV.Models.DTOs.User;
using ChoSV.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
    }
}
