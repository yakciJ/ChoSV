using ChoSV.Models.DTOs.Chat;
using ChoSV.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChoSV.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "UserPolicy")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpGet("history/{otherUserId}")]
        public async Task<IActionResult> GetChatHistory(string otherUserId, int page, int pageSize)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Chưa đăng nhập!");
            }

            var result = await _chatService.GetChatHistoryAsync(userId, otherUserId, page, pageSize);
            return Ok(result);
        }

        [HttpGet("unread")]
        public async Task<IActionResult> GetNewestUnreadMessage()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Chưa đăng nhập!");
            }

            var messages = await _chatService.GetNewestUnreadMessageAsync(userId);
            return Ok(messages);
        }

        [HttpGet("unread/count")]
        public async Task<IActionResult> GetUnreadMessageCount()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Chưa đăng nhập!");
            }

            var count = await _chatService.GetUnreadMessagesCountAsync(userId);
            return Ok(new { count });
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentChats()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Chưa đăng nhập!");
            }

            var recentChats = await _chatService.GetRecentChatsAsync(userId);
            return Ok(recentChats);
        }
        // cái này cũng thừa
        [HttpPost("mark-read/{messageId}")]
        public async Task<IActionResult> MarkAsRead(int messageId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Chưa đăng nhập!");
            }

            await _chatService.MarkAsReadAsync(userId, messageId);
            return Ok(new { message = "Tin nhắn đã được đánh dấu đã đọc" });
        }
        // cái này hình như hơi thừa
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage(SendMessageDTO sendMessageDTO)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Chưa đăng nhập!");
            }

            try
            {
                var message = await _chatService.SendMessageAsync(userId, sendMessageDTO);
                return Ok(new { message = "Gửi tin nhắn thành công", data = message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
