using ChoSV.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChoSV.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "UserPolicy")]
    public class FavoriteController : ControllerBase
    {
        private readonly IFavoriteService _favoriteService;
        public FavoriteController(IFavoriteService favoriteService)
        {
            _favoriteService = favoriteService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFavoriteProducts([FromQuery] int page, [FromQuery] int pageSize)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            var favoriteProducts = await _favoriteService.GetAllFavoriteProductsAsync(userId, page, pageSize);
            return Ok(favoriteProducts);
        }

        [HttpPost]
        public async Task<IActionResult> AddFavoriteAsync(int productId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            await _favoriteService.AddFavoriteAsync(productId, userId);
            return Ok(new { message = "Thêm sản phẩm yêu thích thành công!" });
        }

        [HttpDelete("{productId}")]
        public async Task<IActionResult> DeleteFavoriteAsync(int productId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            await _favoriteService.DeleteFavoriteAsync(productId, userId);
            return Ok(new { message = "Xóa sản phẩm yêu thích thành công!" });
        }
    }
}
