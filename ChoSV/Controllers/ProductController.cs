using ChoSV.Models.DTOs.Product;
using ChoSV.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;



namespace ChoSV.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        // xem  bài của bản thân (tính cả bài chưa duyệt, bài ẩn,..) nhưng giống admin, xem đc trạng thái các kiểu

        // tổng 2 api.
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchAndFilterProductAsync([FromQuery] string? search, int? categoryId, decimal? minPrice, decimal? maxPrice, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var products = await _productService.SearchAndFilterProductsAsync(search, categoryId, minPrice, maxPrice, page, pageSize, userId);
            return Ok(products);
        }

        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProductByIdAsync(int productId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var product = await _productService.GetProductByIdAsync(productId, userId);
            return Ok(product);
        }

        [HttpGet("me")]
        [Authorize(Policy = "UserPolicy")]
        public async Task<IActionResult> GetCurrentUserProductAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Không thể xác định người dùng!");
            }
            var products = await _productService.GetCurrentUserProductAsync(userId, page, pageSize);
            return Ok(products);
        }

        [HttpGet("user/{targetUserId}")]
        public async Task<IActionResult> GetUserProductPostAsync(string targetUserId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //if (string.IsNullOrEmpty(targetUserId))
            //{
            //    return Unauthorized("Không thể xác định người dùng!");
            //}
            var productsList = await _productService.GetUserProductPostsAsync(targetUserId, currentUserId, page, pageSize);
            return Ok(productsList);
        }

        [HttpPost]
        [Authorize(Policy = "UserPolicy")]
        public async Task<IActionResult> CreateProductPostAsync([FromBody] CreateProductPostDTO createProductPostDTO)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Không thể xác định người dùng!");
            }
            await _productService.CreateProductPostAsync(userId, createProductPostDTO);
            return Ok(new { message = "Tạo sản phẩm thành công!" });
        }

        [HttpPut("{productId}")]
        [Authorize(Policy = "UserPolicy")]
        public async Task<IActionResult> UpdateProductPostAsync(int productId, CreateProductPostDTO createProductPostDTO)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Không thể xác định người dùng!");
            }
            await _productService.UpdateProductPostAsync(userId, productId, createProductPostDTO);
            return Ok(new { message = "Cập nhập sản phẩm thành công!" });
        }

        [HttpDelete("user/{productId}")]
        [Authorize(Policy = "UserPolicy")]
        public async Task<IActionResult> DeleteProductPostAsync(int productId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Không thể xác định người dùng!");
            }
            await _productService.DeleteProductPostAsync(userId, productId);
            return Ok(new { message = "Xóa sản phẩm thành công!" });
        }

        [HttpGet("admin/all")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> AdminGetAllProductAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? status = null)
        {
            var pagedResult = await _productService.AdminGetAllProductAsync(page, pageSize, status);
            return Ok(pagedResult);
        }

        [HttpGet("admin/{productId}")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> AdminGetProductAsync(int productId)
        {
            var product = await _productService.AdminGetProductAsync(productId);
            return Ok(product);
        }

        [HttpPut("{productId}/status")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> AdminUpdateProductStatusAsync(int productId, [FromBody] string status)
        {
            await _productService.AdminUpdateProductStatusAsync(productId, status);
            return Ok(new { message = "Cập nhật trạng thái sản phẩm thành công!" });
        }

        [HttpDelete("admin/{productId}")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> AdminDeleleProductPostAsync(int productId)
        {
            await _productService.AdminDeleteProductPostAsync(productId);
            return Ok(new { message = "Xóa sản phẩm thành công!" });
        }
    }
}
