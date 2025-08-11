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
            return Ok(new { message = "Successfully registered" });
        }
    }
}
