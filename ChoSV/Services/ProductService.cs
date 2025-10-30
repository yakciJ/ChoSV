using ChoSV.Configurations;
using ChoSV.Data;
using ChoSV.Models.DTOs.Common;
using ChoSV.Models.DTOs.Product;
using ChoSV.Models.Entities;
using ChoSV.Models.Mappers;
using ChoSV.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace ChoSV.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDBContext _dbContext;
        private readonly ICategoryService _categoryService;
        private readonly IImageService _imageService;
        private readonly INotificationService _notificationService;
        private readonly HttpClient _httpClient;
        private readonly AISettings _aiSettings;
        public ProductService(ApplicationDBContext dbContext, ICategoryService categoryService, IImageService imageService, INotificationService notificationService, HttpClient httpClient,
            IOptions<AISettings> aiSettings)
        {
            _dbContext = dbContext;
            _categoryService = categoryService;
            _imageService = imageService;
            _notificationService = notificationService;
            _httpClient = httpClient; // ✅ Use injected HttpClient
            _aiSettings = aiSettings.Value; // ✅ Use injected settings
        }

        public async Task<PagedResult<ProductListItemDTO>> SearchAndFilterProductsAsync(string? search, int? categoryId, decimal? minPrice, decimal? maxPrice, int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            // Validate price range
            if (maxPrice.HasValue && minPrice.HasValue && maxPrice < minPrice)
            {
                throw new ArgumentException("Lọc không hợp lệ!");
            }

            // Check if we have any search criteria
            bool hasFilters = categoryId.HasValue || minPrice.HasValue || maxPrice.HasValue;
            if (search == null && !hasFilters)
            {
                throw new ArgumentException("Từ khóa không hợp lệ!");
            }

            var filteredProductIds = new List<int>();

            if (hasFilters)
            {
                var query = _dbContext.Products
                        .AsNoTracking()
                        .Where(p => p.Status == "Approved" || p.Status == "Sold");

                if (search == null)
                    query = query
                        .Include(p => p.Seller)
                        .Include(p => p.ProductImages)
                        .Include(p => p.Favorites)
                        .Include(p => p.Categories);

                if (categoryId.HasValue)
                {
                    query = query.Where(p => p.Categories.Any(c => c.CategoryId == categoryId.Value));
                }

                if (minPrice.HasValue)
                {
                    query = query.Where(p => p.Price >= minPrice.Value);
                }

                if (maxPrice.HasValue)
                {
                    query = query.Where(p => p.Price <= maxPrice.Value);
                }

                if (search != null)
                {
                    filteredProductIds = await query.Select(p => p.ProductId).ToListAsync();
                    if (filteredProductIds.Count == 0)
                    {
                        return new PagedResult<ProductListItemDTO>
                        {
                            Items = new List<ProductListItemDTO>(),
                            TotalCount = 0,
                            Page = page,
                            PageSize = pageSize
                        };
                    }
                }
                else
                {
                    var totalCount = await query.CountAsync();

                    if (totalCount == 0)
                    {
                        return new PagedResult<ProductListItemDTO>
                        {
                            Items = new List<ProductListItemDTO>(),
                            TotalCount = 0,
                            Page = page,
                            PageSize = pageSize
                        };
                    }
                    var products = await query
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

                    var productDTOs = products.Select(p => p.ToProductListItemDTO()).ToList();

                    return new PagedResult<ProductListItemDTO>
                    {
                        Items = productDTOs,
                        TotalCount = totalCount,
                        Page = page,
                        PageSize = pageSize
                    };
                }
            }

            var aiResponse = await CallAISearchServiceAsync(search, filteredProductIds, page, pageSize);
            if (aiResponse?.Results?.Any() == true)
            {
                // Get products in the order returned by AI service
                var orderedProducts = new List<Product>();
                foreach (var productId in aiResponse.Results)
                {
                    var product = await _dbContext.Products
                        .AsNoTracking()
                        .Include(p => p.Seller)
                        .Include(p => p.ProductImages)
                        .Include(p => p.Favorites)
                        .Include(p => p.Categories)
                        .FirstOrDefaultAsync(p => p.ProductId == productId);

                    if (product != null)
                    {
                        orderedProducts.Add(product);
                    }
                }

                var aiProductDTOs = orderedProducts.Select(p => p.ToProductListItemDTO(null)).ToList();

                return new PagedResult<ProductListItemDTO>
                {
                    Items = aiProductDTOs,
                    TotalCount = aiProductDTOs.Count,
                    Page = page,
                    PageSize = pageSize
                };
            }

            return new PagedResult<ProductListItemDTO>
            {
                Items = new List<ProductListItemDTO>(),
                TotalCount = 0,
                Page = page,
                PageSize = pageSize
            };
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

            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("x-api-key", _aiSettings.ApiKey);

                // Encode dữ liệu tiếng Việt để tránh lỗi URL
                var encodedTitle = Uri.EscapeDataString(product.ProductName);
                var selectedCategoryId = createProductPostDTO.CategoryIds.First();
                var selectedCategory = categories.FirstOrDefault(c => c.CategoryId == selectedCategoryId);
                var encodedCategory = Uri.EscapeDataString(selectedCategory?.Name ?? "Khác");

                var aiUrl = $"{_aiSettings.BaseUrl}/insert?id={product.ProductId}&title={encodedTitle}&category={encodedCategory}";

                var response = await _httpClient.PostAsync(aiUrl, null);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"⚠️ AI sync failed: {(int)response.StatusCode} {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error calling AI Service: {ex.Message}");
            }
        }

        public async Task UpdateProductPostAsync(string userId, int productId, CreateProductPostDTO createProductPostDTO)
        {
            var product = await _dbContext.Products
                .Include(p => p.ProductImages)
                .Include(p => p.Categories)
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
            product.Categories.Clear();
            foreach (var category in categories)
            {
                product.Categories.Add(category);
            }

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

            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("x-api-key", _aiSettings.ApiKey);

                // Encode dữ liệu tiếng Việt để tránh lỗi URL
                var encodedTitle = Uri.EscapeDataString(product.ProductName);
                var selectedCategoryId = createProductPostDTO.CategoryIds.First();
                var selectedCategory = categories.FirstOrDefault(c => c.CategoryId == selectedCategoryId);
                var encodedCategory = Uri.EscapeDataString(selectedCategory?.Name ?? "Khác");

                var aiUrl = $"{_aiSettings.BaseUrl}/update?id={product.ProductId}&title={encodedTitle}&category={encodedCategory}";

                var response = await _httpClient.PutAsync(aiUrl, null);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"⚠️ AI sync failed: {(int)response.StatusCode} {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error calling AI Service: {ex.Message}");
            }

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

            await _notificationService.SendProductNotificationAsync(product);
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

            var userId = product.SellerId;
            var productName = product.ProductName;

            _dbContext.Products.Remove(product);
            await _dbContext.SaveChangesAsync();

            await _notificationService.SendProductNotificationAsync(userId, productName);
        }

        private async Task<AISearchResponseDTO?> CallAISearchServiceAsync(string search, List<int>? productIds, int page, int pageSize)
        {
            try
            {
                // Set the API key header
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("x-api-key", _aiSettings.ApiKey);

                // Build the URL
                var productIdsString = string.Join(",", productIds ?? new List<int>());

                var url = $"{_aiSettings.BaseUrl}/search?q={Uri.EscapeDataString(search)}&page={page}&page_size={pageSize}&product_ids={productIdsString}";

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var aiResponse = JsonSerializer.Deserialize<AISearchResponseDTO>(jsonResponse, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return aiResponse;
                }

                return null;
            }
            catch (Exception ex)
            {
                // Log the exception (you should use proper logging)
                Console.WriteLine($"Error calling AI Search Service: {ex.Message}");
                return null;
            }
        }
    }
}
