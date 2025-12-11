using ChoSV.Models.DTOs.Category;
using ChoSV.Models.Entities;

namespace ChoSV.Models.Mappers
{
    public static class CategoryMapper
    {
        public static Category ToCategoryFromCreateCategoryDTO(this CreateCategoryDTO createCategoryDTO)
        {
            return new Category
            {
                Name = createCategoryDTO.Name,
                Description = createCategoryDTO.Description,
                ImageUrl = createCategoryDTO.ImageUrl,
                ParentCategoryId = createCategoryDTO.ParentCategoryId,
            };
        }

        public static CategoryTreeDTO ToCategoryTreeDTO(this Category category)
        {
            return new CategoryTreeDTO
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                ImageUrl = category.ImageUrl,
                Childs = new List<CategoryTreeDTO>()
            };
        }

        public static CategoryDTO ToCategoryDTOFromCategory(this Category category)
        {
            return new CategoryDTO
            {
                CategoryId = category.CategoryId,
                CategoryName = category.Name,
                ImageUrl = category.ImageUrl,
            };
        }

        public static void ToCategoryFromUpdateCategoryDTO(this Category category, UpdateCategoryDTO updateCategoryDTO)
        {
            category.Name = updateCategoryDTO.Name;
            category.Description = updateCategoryDTO.Description;
            category.ImageUrl = updateCategoryDTO.ImageUrl;
            category.ParentCategoryId = updateCategoryDTO.ParentCategoryId;
        }
    }
}
