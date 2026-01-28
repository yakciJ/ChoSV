using ChoSV.Models.DTOs.Category;
using ChoSV.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChoSV.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet("{categoryId}/products/")]
        public async Task<IActionResult> GetCategoryByIdAsync(int categoryId, [FromQuery] int page, [FromQuery] int pageSize)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _categoryService.GetCategoryByIdAsync(categoryId, page, pageSize, userId);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCategoriesAsync()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("tree")]
        public async Task<IActionResult> GetCategoryTreesAsync()
        {
            var categoryTrees = await _categoryService.GetCategoryTreesAsync();
            return Ok(categoryTrees);
        }

        [HttpPost]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> CreateCategoryAsync(CreateCategoryDTO createCategoryDTO)
        {
            await _categoryService.CreateCategoryAsync(createCategoryDTO);
            return Ok(new { message = "Tạo danh mục thành công!" });
        }

        [HttpPut]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> UpdateCategoryAsync(UpdateCategoryDTO updateCategoryDTO)
        {
            await _categoryService.UpdateCategoryAsync(updateCategoryDTO);
            return Ok(new { message = "Cập nhập danh mục thành công!" });
        }
        [HttpDelete("{categoryId}")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> DeleteCategoryAsync(int categoryId)
        {
            await _categoryService.DeleteCategoryAsync(categoryId);
            return Ok(new { message = "Xóa danh mục thành công!" });
        }
    }
}
