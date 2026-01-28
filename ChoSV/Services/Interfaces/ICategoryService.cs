using ChoSV.Models.DTOs.Category;

namespace ChoSV.Services.Interfaces
{
    public interface ICategoryService
    {
        // sửa thg get đầu cho lấy full cha của nó
        Task<CategoryDetailDTO> GetCategoryByIdAsync(int categoryId, int page, int pageSize, string? userId);
        Task<List<CategoryDTO>> GetAllCategoriesAsync();
        Task<List<CategoryTreeDTO>> GetCategoryTreesAsync();
        Task CreateCategoryAsync(CreateCategoryDTO createCategoryDTO);
        Task UpdateCategoryAsync(UpdateCategoryDTO updateCategoryDTO);
        Task DeleteCategoryAsync(int categoryId);
        Task<List<int>> GetCategoryIdsWithParentsAsync(List<int> selectedCategoryIds);
        Task<bool> ValidateCategoryIdsAsync(List<int> categoryIds);
    }
}
