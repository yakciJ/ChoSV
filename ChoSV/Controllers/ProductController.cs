using ChoSV.Models.DTOs.Product;
using ChoSV.Models.Entities;
using ChoSV.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;



namespace ChoSV.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IProductService _productService;

        public ProductController(IProductService productService, UserManager<User> userManager)
        {
            _userManager = userManager;
            _productService = productService;
        }

        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProductByIdAsync(int productId)
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.NameId)?.Value;
            var product = await _productService.GetProductByIdAsync(productId, userId);
            return Ok(product);
        }

        [HttpPost]
        [Authorize(Policy = "UserPolicy")]
        public async Task<IActionResult> CreateProductPost([FromForm] CreateProductPostDTO createProductPostDTO)
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.NameId)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Không thể xác định người dùng!");
            }
            await _productService.CreateProductPostAsync(userId, createProductPostDTO);
            return Ok(new { message = "Tạo sản phẩm thành công!" });
        }
    }
}
