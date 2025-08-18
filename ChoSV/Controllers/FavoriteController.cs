using ChoSV.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace ChoSV.Controllers
{
    [Route("api/{controller}")]
    [ApiController]
    public class FavoriteController : ControllerBase
    {
        private readonly IFavoriteService _favoriteService;
        public FavoriteController(IFavoriteService favoriteService)
        {
            _favoriteService = favoriteService;
        }

        [HttpGet]
        [Authorize(Policy = "UserPolicy")]
        public async Task<IActionResult> GetAllFavoriteProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.NameId)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            var favoriteProducts = await _favoriteService.GetAllFavoriteProductsAsync(userId, page, pageSize);
            return Ok(favoriteProducts);
        }

        [HttpPost]
        [Authorize(Policy = "UserPolicy")]
        public async Task<IActionResult> AddFavoriteAsync(int productId)
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.NameId)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            await _favoriteService.AddFavoriteAsync(productId, userId);
            return Ok(new { message = "Thêm sản phẩm yêu thích thành công!" });
        }

        [HttpDelete]
        [Authorize(Policy = "UserPolicy")]
        public async Task<IActionResult> DeleteFavoriteAsync(int productId)
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.NameId)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            await _favoriteService.DeleteFavoriteAsync(productId, userId);
            return Ok(new { message = "Xóa sản phẩm yêu thích thành công!" });
        }
    }
}
