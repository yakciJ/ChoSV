using ChoSV.Data;
using ChoSV.Models.DTOs.Common;
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

        public async Task<PagedResult<ProductDetailListDTO>> GetCurrentUserProductAsync(string userId, int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var query = _dbContext.Products
                .AsNoTracking()
                .Include(p => p.Favorites)
                .Include(p => p.Seller)
                .Include(p => p.ProductImages)
                .Include(p => p.Categories)
                .Where(p => p.SellerId == userId)
                .OrderByDescending(p => p.CreatedDate);

            var totalCount = await query.CountAsync();

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var productDTOs = products.Select(p => p.ToProductDetailListDTO()).ToList();

            return new PagedResult<ProductDetailListDTO>
            {
                Items = productDTOs,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<PagedResult<ProductListItemDTO>> GetUserProductPostsAsync(string userId, string? currentUserId, int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var query = _dbContext.Products
                .AsNoTracking()
                .Include(p => p.Seller)
                .Include(p => p.ProductImages)
                .Include(p => p.Favorites)
                .Include(p => p.Categories)
                .Where(p => p.SellerId == userId && (p.Status == "Approved" || p.Status == "Sold"))
                .OrderByDescending(p => p.CreatedDate);

            var totalCount = await query.CountAsync();

            //if (totalCount == 0)
            //{
            //    throw new ArgumentException("Không có sản phẩm nào!");
            //}

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var productDTOs = products.Select(p => p.ToProductListItemDTO(currentUserId)).ToList();

            return new PagedResult<ProductListItemDTO>
            {
                Items = productDTOs,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
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

            if (createProductPostDTO.ImagesUrl?.Any() == true)
            {
                var productImages = new List<ProductImage>();
                foreach (var imageUrl in createProductPostDTO.ImagesUrl)
                {
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
        }

        public async Task UpdateProductPostAsync(string userId, int productId, CreateProductPostDTO createProductPostDTO)
        {
            var product = await _dbContext.Products
                .Include(p => p.SellerId)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.ProductId == productId);
            if (product == null)
            {
                throw new ArgumentException("Sản phẩm không tồn tại!");
            }
            if (userId != product.SellerId)
            {
                throw new ArgumentException("Không có quyền!");
            }
            if (!await _categoryService.ValidateCategoryIdsAsync(createProductPostDTO.CategoryIds))
            {
                throw new ArgumentException("Một hoặc nhiều danh mục không hợp lệ!");
            }
            var allCategoryIds = await _categoryService.GetCategoryIdsWithParentsAsync(createProductPostDTO.CategoryIds);
            var categories = await _dbContext.Categories
                .Where(c => allCategoryIds.Contains(c.CategoryId))
                .ToListAsync();

            product.UpdateProductPost(createProductPostDTO);
            product.Categories = categories;

            var existingImageUrls = product.ProductImages.Select(pi => pi.ImageUrl).ToList();

            // Compare with incoming URLs from DTO
            var incomingImageUrls = createProductPostDTO.ImagesUrl ?? new List<string>();

            // Find URLs to add and remove
            var urlsToAdd = incomingImageUrls.Except(existingImageUrls).ToList();
            var urlsToDelete = existingImageUrls.Except(incomingImageUrls).ToList();

            // Delete removed images
            foreach (var urlToDelete in urlsToDelete)
            {
                // Delete the physical file
                _imageService.DeleteImageByUrl(urlToDelete);

                // Remove from database
                var imageToRemove = product.ProductImages.FirstOrDefault(pi => pi.ImageUrl == urlToDelete);
                if (imageToRemove != null)
                {
                    _dbContext.ProductImages.Remove(imageToRemove);
                }
            }

            // Add new images
            foreach (var urlToAdd in urlsToAdd)
            {
                var newProductImage = new ProductImage
                {
                    ProductId = productId,
                    ImageUrl = urlToAdd
                };
                _dbContext.ProductImages.Add(newProductImage);
            }

            await _dbContext.SaveChangesAsync();

        }

        public async Task DeleteProductPostAsync(string userId, int productId)
        {
            var product = await _dbContext.Products
                .Include(p => p.ProductImages)
                .Include(p => p.Favorites)
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null)
            {
                throw new ArgumentException("Sản phẩm không tồn tại!");
            }
            if (product.SellerId != userId)
            {
                throw new ArgumentException("Chỉ xóa được sản phẩm của mình!");
            }

            if (product.ProductImages?.Any() == true)
            {
                foreach (var image in product.ProductImages)
                {
                    _imageService.DeleteImageByUrl(image.ImageUrl);
                }
            }

            if (product.Favorites?.Any() == true)
            {
                _dbContext.Favorites.RemoveRange(product.Favorites);
            }

            if (product.ProductImages?.Any() == true)
            {
                _dbContext.ProductImages.RemoveRange(product.ProductImages);
            }

            _dbContext.Products.Remove(product);
            await _dbContext.SaveChangesAsync();
        }


        public async Task<PagedResult<ProductDetailListDTO>> AdminGetAllProductAsync(int page = 1, int pageSize = 10, string? status = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var validStatuses = new[] { "Pending", "Approved", "Rejected", "Sold", "Archived", "Deleted" };
            if (!string.IsNullOrEmpty(status) && !validStatuses.Contains(status))
            {
                throw new ArgumentException($"Invalid status: {status}. Valid statuses are: {string.Join(", ", validStatuses)}");
            }

            var query = _dbContext.Products
                .AsNoTracking()
                .Include(p => p.Seller)
                .Include(p => p.ProductImages)
                .Include(p => p.Categories)
                .Where(p => string.IsNullOrEmpty(status) || p.Status == status)
                .OrderByDescending(p => p.CreatedDate);

            var totalCount = await query.CountAsync();

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var productDTOs = products.Select(p => p.ToProductDetailListDTO()).ToList();

            return new PagedResult<ProductDetailListDTO>
            {
                Items = productDTOs,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task AdminUpdateProductStatusAsync(int productId, string status)
        {
            var validStatuses = new[] { "Pending", "Approved", "Rejected", "Sold", "Archived", "Deleted" };
            if (string.IsNullOrEmpty(status) || !validStatuses.Contains(status))
            {
                throw new ArgumentException($"Invalid status: {status}. Valid statuses are: {string.Join(", ", validStatuses)}");
            }

            var product = await _dbContext.Products.FindAsync(productId);
            if (product == null)
            {
                throw new ArgumentException("Sản phẩm không tồn tại!");
            }

            product.Status = status;
            await _dbContext.SaveChangesAsync();
        }

        public async Task AdminDeleteProductPostAsync(int productId)
        {
            var product = await _dbContext.Products
                .Include(p => p.ProductImages)
                .Include(p => p.Favorites)
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null)
            {
                throw new ArgumentException("Sản phẩm không tồn tại!");
            }

            if (product.ProductImages?.Any() == true)
            {
                foreach (var image in product.ProductImages)
                {
                    _imageService.DeleteImageByUrl(image.ImageUrl);
                }
            }

            if (product.Favorites?.Any() == true)
            {
                _dbContext.Favorites.RemoveRange(product.Favorites);
            }

            if (product.ProductImages?.Any() == true)
            {
                _dbContext.ProductImages.RemoveRange(product.ProductImages);
            }

            _dbContext.Products.Remove(product);
            await _dbContext.SaveChangesAsync();
        }
    }
}
