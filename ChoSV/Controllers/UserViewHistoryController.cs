using ChoSV.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChoSV.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "UserPolicy")]
    public class UserViewHistoryController : ControllerBase
    {
        private readonly IUserViewHistory _userViewHistory;
        public UserViewHistoryController(IUserViewHistory userViewHistory)
        {
            _userViewHistory = userViewHistory;
        }
        [HttpPost("view/{productId}")]
        public async Task<IActionResult> ViewProduct(int productId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Chưa đăng nhập!");
            }
            await _userViewHistory.SawProduct(userId, productId);
            return Ok(new { message = "Cập nhập lịch sử xem thành công!" });
        }
        [HttpDelete("delete/{productId}")]
        public async Task<IActionResult> DeleteHistory(int productId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Chưa đăng nhập!");
            }
            await _userViewHistory.DeleteHistory(userId, productId);
            return Ok(new { message = "Xóa lịch sử xem thành công!" });
        }
        [HttpDelete("delete-all")]
        public async Task<IActionResult> DeleteHistories()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Chưa đăng nhập!");
            }
            await _userViewHistory.DeleteHistories(userId);
            return Ok(new { message = "Xóa lịch sử xem thành công!" });
        }
        [HttpGet("my-history")]
        public async Task<IActionResult> GetMyViewHistory([FromQuery] int page, [FromQuery] int pageSize)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Chưa đăng nhập!");
            }
            var histories = await _userViewHistory.GetUserViewHistories(userId, page, pageSize);
            return Ok(histories);
        }
    }
}
