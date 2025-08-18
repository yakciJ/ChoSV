using ChoSV.Data;
using ChoSV.Models.DTOs.Category;
using ChoSV.Models.DTOs.Common;
using ChoSV.Models.DTOs.Product;
using ChoSV.Models.Entities;
using ChoSV.Models.Mappers;
using ChoSV.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ChoSV.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDBContext _dbContext;
        public CategoryService(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CategoryDetailDTO> GetCategoryByIdAsync(int categoryId, int page = 1, int pageSize = 10, string? userId = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 1;
            if (pageSize > 100) pageSize = 100;

            var category = await _dbContext.Categories
                .Include(c => c.ParentCategory)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);
            if (category == null)
                throw new ArgumentException("Danh mục không tồn tại!");

            var totalProductCount = await _dbContext.Products
                .Where(p => p.Categories.Any(c => c.CategoryId == categoryId) &&
                   (p.Status == "Approved" || p.Status == "Sold"))
                .CountAsync();

            int skip = (page - 1) * pageSize;

            var products = await _dbContext.Products
                .Where(p => p.Categories.Any(c => c.CategoryId == categoryId) &&
                   (p.Status == "Approved" || p.Status == "Sold"))
                .Include(p => p.Seller)
                .Include(p => p.ProductImages)
                .Include(p => p.Favorites)
                .OrderByDescending(p => p.CreatedDate) // Most recent first
                .Skip(skip)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            var productDTOs = products.Select(p => p.ToProductListItemDTO(userId)).ToList();

            var pagedProducts = new PagedResult<ProductListItemDTO>
            {
                Items = productDTOs,
                TotalCount = totalProductCount,
                Page = page,
                PageSize = pageSize
            };

            return new CategoryDetailDTO
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Description = category.Description,
                ProductCount = totalProductCount,
                Products = pagedProducts
            };
        }

        public async Task<List<CategoryDTO>> GetAllCategoriesAsync()
        {
            return await _dbContext.Categories
                .AsNoTracking()
                .Select(c => c.ToCategoryDTOFromCategory())
                .ToListAsync();
        }

        public async Task<List<CategoryTreeDTO>> GetCategoryTreesAsync()
        {
            // Get all categories with their relationships
            var allCategories = await _dbContext.Categories
                .Include(c => c.ChildCategories)
                .AsNoTracking()
                .ToListAsync();

            // Find root categories (categories with no parent)
            var rootCategories = allCategories
                .Where(c => c.ParentCategoryId == null)
                .ToList();

            // Build the tree structure
            var categoryTrees = new List<CategoryTreeDTO>();

            foreach (var rootCategory in rootCategories)
            {
                var treeNode = BuildCategoryTree(rootCategory, allCategories);
                categoryTrees.Add(treeNode);
            }

            return categoryTrees;
        }

        private CategoryTreeDTO BuildCategoryTree(Category category, List<Category> allCategories)
        {
            var treeNode = category.ToCategoryTreeDTO();

            // Find all direct children of this category
            var children = allCategories
                .Where(c => c.ParentCategoryId == category.CategoryId)
                .ToList();

            // Recursively build child trees
            foreach (var child in children)
            {
                var childTree = BuildCategoryTree(child, allCategories);
                treeNode.Childs.Add(childTree);
            }

            return treeNode;
        }

        public async Task CreateCategoryAsync(CreateCategoryDTO createCategoryDTO)
        {
            // Validate input
            if (createCategoryDTO == null)
                throw new ArgumentException(nameof(createCategoryDTO));

            if (string.IsNullOrWhiteSpace(createCategoryDTO.Name))
                throw new ArgumentException("Tên không được để trống!", nameof(createCategoryDTO));

            // Check for duplicate category name
            var existingCategory = await _dbContext.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == createCategoryDTO.Name.ToLower());

            if (existingCategory != null)
                throw new ArgumentException($"Một danh mục có tên '{createCategoryDTO.Name}' đã tồn tại.");

            // Validate parent category if specified
            if (createCategoryDTO.ParentCategoryId.HasValue)
            {
                var parentExists = await _dbContext.Categories
                    .AnyAsync(c => c.CategoryId == createCategoryDTO.ParentCategoryId.Value);

                if (!parentExists)
                    throw new ArgumentException($"Danh mục cha  {createCategoryDTO.ParentCategoryId} không tồn tại.");
            }

            var category = createCategoryDTO.ToCategoryFromCreateCategoryDTO();
            _dbContext.Categories.Add(category);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateCategoryAsync(UpdateCategoryDTO updateCategoryDTO)
        {
            if (updateCategoryDTO == null)
                throw new ArgumentException(nameof(updateCategoryDTO));

            if (string.IsNullOrWhiteSpace(updateCategoryDTO.Name))
                throw new ArgumentException("Tên không được để trống!", nameof(updateCategoryDTO));

            // Check for duplicate category name
            var existingCategory = await _dbContext.Categories
        .FirstOrDefaultAsync(c => c.Name.ToLower() == updateCategoryDTO.Name.ToLower()
                                && c.CategoryId != updateCategoryDTO.CategoryId);


            if (existingCategory != null)
                throw new ArgumentException($"Một danh mục có tên '{updateCategoryDTO.Name}' đã tồn tại.");

            // Validate parent category if specified
            if (updateCategoryDTO.ParentCategoryId.HasValue)
            {
                if (updateCategoryDTO.ParentCategoryId.Value == updateCategoryDTO.CategoryId)
                    throw new ArgumentException("Danh mục không thể là danh mục cha của chính nó.");

                var parentExists = await _dbContext.Categories
                    .AnyAsync(c => c.CategoryId == updateCategoryDTO.ParentCategoryId.Value);

                if (!parentExists)
                    throw new ArgumentException($"Danh mục cha  {updateCategoryDTO.ParentCategoryId} không tồn tại.");

                if (await WouldCreateCircularReference(updateCategoryDTO.CategoryId, updateCategoryDTO.ParentCategoryId.Value))
                    throw new ArgumentException("Không thể tạo quan hệ cha-con vòng tròn.");

            }
            var category = await _dbContext.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == updateCategoryDTO.CategoryId);
            if (category == null)
            {
                throw new ArgumentException("Danh mục không tồn tại!");
            }
            category.ToCategoryFromUpdateCategoryDTO(updateCategoryDTO);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(int categoryId)
        {
            var category = await _dbContext.Categories
                .Include(c => c.Products)
                .Include(c => c.ChildCategories)
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);
            if (category == null)
            {
                throw new ArgumentException("Danh mục không tồn tại!");
            }
            if (category.Products.Any())
            {
                foreach (var product in category.Products.ToList())
                {
                    product.Categories.Remove(category);
                }
            }
            if (category.ChildCategories?.Any() == true)
            {
                foreach (var childCategory in category.ChildCategories)
                {
                    childCategory.ParentCategoryId = category.ParentCategoryId;
                }
            }

            _dbContext.Categories.Remove(category);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<int>> GetCategoryIdsWithParentsAsync(List<int> selectedCategoryIds)
        {
            var allCategoryIds = new HashSet<int>();

            foreach (var categoryId in selectedCategoryIds)
            {
                var categoryWithParents = await GetCategoryWithParentsAsync(categoryId);
                foreach (var cat in categoryWithParents)
                {
                    allCategoryIds.Add(cat.CategoryId);
                }
            }
            return allCategoryIds.ToList();
        }

        public async Task<List<Category>> GetCategoryWithParentsAsync(int categoryId)
        {
            var categories = new List<Category>();
            var currentCategory = await _dbContext.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

            if (currentCategory == null)
            {
                throw new ArgumentException($"Category with ID {categoryId} not found!");
            }

            while (currentCategory != null)
            {
                categories.Add(currentCategory);
                if (currentCategory.ParentCategoryId.HasValue)
                {
                    currentCategory = await _dbContext.Categories
                        .Include(c => c.ParentCategory)
                        .FirstOrDefaultAsync(c => c.CategoryId == currentCategory.ParentCategoryId);
                }
                else currentCategory = null;
            }
            return categories;
        }

        public async Task<bool> ValidateCategoryIdsAsync(List<int> categoryIds)
        {
            if (!categoryIds.Any()) return false;
            var existingCount = await _dbContext.Categories
                .Where(c => categoryIds.Contains(c.CategoryId))
                .CountAsync();
            return existingCount == categoryIds.Count;
        }

        private async Task<bool> WouldCreateCircularReference(int categoryId, int parentCategoryId)
        {
            int? currentParentId = parentCategoryId;

            while (currentParentId.HasValue)
            {
                if (currentParentId.Value == categoryId)
                    return true;

                var parent = await _dbContext.Categories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.CategoryId == currentParentId.Value);

                currentParentId = parent?.ParentCategoryId;
            }

            return false;
        }
    }
}
