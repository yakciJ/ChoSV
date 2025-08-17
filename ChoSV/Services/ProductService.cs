using ChoSV.Data;
using ChoSV.Models.DTOs.Product;
using ChoSV.Models.Entities;
using ChoSV.Models.Mappers;
using ChoSV.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChoSV.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDBContext _dbContext;
        private readonly ICategoryService _categoryService;
        private readonly IImageService _imageService;
        public ProductService(ApplicationDBContext dbContext, ICategoryService categoryService, IImageService imageService)
        {
            _dbContext = dbContext;
            _categoryService = categoryService;
            _imageService = imageService;
        }

        public async Task<ProductDetailsDTO> GetProductByIdAsync(int productId, string? userId)
        {
            var product = await _dbContext.Products
                .AsNoTracking()
                .Include(p => p.Seller)
                .Include(p => p.ProductImages)
                .Include(p => p.Favorites)
                .FirstOrDefaultAsync(p => p.ProductId == productId);
            if (product == null)
            {
                throw new ArgumentException("Không tìm thấy sản phẩm!");
            }
            if ((product.Status != "Approved" && product.Status != "Sold") && (string.IsNullOrEmpty(userId) || userId != product.SellerId))
            {
                throw new ArgumentException("Không tìm thấy sản phẩm!");
            }
            return product.ToProductDetailsDTO(userId);
        }

        public async Task CreateProductPostAsync(string userId, CreateProductPostDTO createProductPostDTO)
        {
            if (!await _categoryService.ValidateCategoryIdsAsync(createProductPostDTO.CategoryIds))
            {
                throw new ArgumentException("Một hoặc nhiều danh mục không hợp lệ!");
            }

            var allCategoryIds = await _categoryService.GetCategoryIdsWithParentsAsync(createProductPostDTO.CategoryIds);
            var categories = await _dbContext.Categories
                .Where(c => allCategoryIds.Contains(c.CategoryId))
                .ToListAsync();

            var product = new Product
            {
                ProductName = createProductPostDTO.ProductName,
                ProductDescription = createProductPostDTO.ProductDescription,
                Price = createProductPostDTO.Price,
                SellerId = userId,
                CreatedDate = DateTime.UtcNow,
                Categories = categories
            };
            await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();

            if (createProductPostDTO.Images?.Any() == true)
            {
                var productImages = new List<ProductImage>();
                var uploadedImageUrls = new List<string>();
                try
                {
                    foreach (var imageFile in createProductPostDTO.Images)
                    {
                        // Upload each image using ImageService
                        var imageUrl = await _imageService.UploadImageAsync(imageFile);
                        uploadedImageUrls.Add(imageUrl);
                        // Create ProductImage entity
                        var productImage = new ProductImage
                        {
                            ProductId = product.ProductId,
                            ImageUrl = imageUrl
                        };

                        productImages.Add(productImage);
                    }
                    await _dbContext.ProductImages.AddRangeAsync(productImages);
                    await _dbContext.SaveChangesAsync();
                }
                catch (ArgumentException ex)
                {
                    foreach (var imageUrl in uploadedImageUrls)
                    {
                        _imageService.DeleteImageByUrl(imageUrl);
                    }
                    throw new ArgumentException($"Lỗi upload ảnh: {ex.Message}");
                }
            }
        }
    }
}
