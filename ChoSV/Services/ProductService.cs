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

        public async Task<PagedResult<ProductListItemDTO>> SearchAndFilterProductsAsync(string? search, int? categoryId, decimal? minPrice, decimal? maxPrice, int page = 1, int pageSize = 10, string? userId = null, string? sortBy = "relevance")
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
            bool hasSearch = !string.IsNullOrWhiteSpace(search);

            if (!hasSearch && !hasFilters)
            {
                throw new ArgumentException("Vui lòng nhập từ khóa tìm kiếm hoặc chọn bộ lọc!");
            }

            var validSortOptions = new[] { "relevance", "price_high", "price_low", "newest" };
            if (!string.IsNullOrEmpty(sortBy) && !validSortOptions.Contains(sortBy.ToLower()))
            {
                throw new ArgumentException("Sắp xếp không hợp lệ!");
            }

            // Case 1: Only filters, no search - Direct database query
            if (!hasSearch && hasFilters)
            {
                var query = _dbContext.Products
                    .AsNoTracking()
                    .Include(p => p.Seller)
                    .Include(p => p.ProductImages)
                    .Include(p => p.Favorites)
                    .Include(p => p.Categories)
                    .Where(p => p.Status == "Approved");

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

                query = ApplySorting(query, sortBy?.ToLower() ?? "newest");

                var totalCount = await query.CountAsync();

                var products = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var productDTOs = products.Select(p => p.ToProductListItemDTO(userId)).ToList();

                return new PagedResult<ProductListItemDTO>
                {
                    Items = productDTOs,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
            }

            // Case 2: Has search (with or without filters) - Use AI service
            var aiResponse = await CallAISearchServiceAsync(search!, categoryId, minPrice, maxPrice, page, pageSize);

            if (aiResponse?.ProductIds == null || !aiResponse.ProductIds.Any())
            {
                return new PagedResult<ProductListItemDTO>
                {
                    Items = new List<ProductListItemDTO>(),
                    TotalCount = 0,
                    Page = page,
                    PageSize = pageSize
                };
            }

            // Fetch products in the order returned by AI service
            var orderedProducts = new List<Product>();
            foreach (var productId in aiResponse.ProductIds)
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

            if (!string.IsNullOrEmpty(sortBy) && sortBy.ToLower() != "relevance")
            {
                orderedProducts = SortProductList(orderedProducts, sortBy.ToLower());
            }

            var aiProductDTOs = orderedProducts.Select(p => p.ToProductListItemDTO(userId)).ToList();

            return new PagedResult<ProductListItemDTO>
            {
                Items = aiProductDTOs,
                TotalCount = aiResponse.TotalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<PagedResult<ProductListItemDTO>> GetNewestProductsAsync(int page = 1, int pageSize = 10, string? userId = null)
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
                .Where(p => p.Status == "Approved")
                .OrderByDescending(p => p.CreatedDate);

            var totalCount = await query.CountAsync();

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var productDTOs = products.Select(p => p.ToProductListItemDTO(userId)).ToList();

            return new PagedResult<ProductListItemDTO>
            {
                Items = productDTOs,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<PagedResult<ProductListItemDTO>> GetPopularProductsAsync(int page = 1, int pageSize = 10, string? userId = null, int daysBack = 30)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var cutoffDate = DateTime.UtcNow.AddDays(-daysBack);

            var query = _dbContext.Products
                .AsNoTracking()
                .Include(p => p.Seller)
                .Include(p => p.ProductImages)
                .Include(p => p.Favorites)
                .Include(p => p.Categories)
                .Where(p => p.Status == "Approved" && p.CreatedDate >= cutoffDate)
                .OrderByDescending(p => p.Favorites.Count)
                .ThenByDescending(p => p.CreatedDate);

            var totalCount = await query.CountAsync();
            var products = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            var productDTOs = products.Select(p => p.ToProductListItemDTO(userId)).ToList();

            return new PagedResult<ProductListItemDTO>
            {
                Items = productDTOs,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<PagedResult<ProductListItemDTO>> GetSimilarProductsAsync(int productId, int page = 1, int pageSize = 10, string? userId = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            // Check if product exists
            var productExists = await _dbContext.Products.AnyAsync(p => p.ProductId == productId);
            if (!productExists)
            {
                throw new ArgumentException("Sản phẩm không tồn tại!");
            }

            // Call AI service to get similar product IDs
            var aiResponse = await CallAISimilarProductsServiceAsync(productId, page, pageSize);

            if (aiResponse?.ProductIds == null || !aiResponse.ProductIds.Any())
            {
                return new PagedResult<ProductListItemDTO>
                {
                    Items = new List<ProductListItemDTO>(),
                    TotalCount = 0,
                    Page = page,
                    PageSize = pageSize
                };
            }

            // Fetch products in the order returned by AI service
            var orderedProducts = new List<Product>();
            foreach (var pid in aiResponse.ProductIds)
            {
                var product = await _dbContext.Products
                    .AsNoTracking()
                    .Include(p => p.Seller)
                    .Include(p => p.ProductImages)
                    .Include(p => p.Favorites)
                    .Include(p => p.Categories)
                    .FirstOrDefaultAsync(p => p.ProductId == pid && (p.Status == "Approved"));

                if (product != null)
                {
                    orderedProducts.Add(product);
                }
            }

            var productDTOs = orderedProducts.Select(p => p.ToProductListItemDTO(userId)).ToList();

            return new PagedResult<ProductListItemDTO>
            {
                Items = productDTOs,
                TotalCount = productDTOs.Count,
                Page = page,
                PageSize = pageSize
            };
        }

        private async Task<AISearchResponseDTO?> CallAISimilarProductsServiceAsync(int productId, int page, int pageSize)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("x-api-key", _aiSettings.ApiKey);

                var url = $"{_aiSettings.BaseUrl}/similar/{productId}?page={page}&page_size={pageSize}";

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<AISearchResponseDTO>(jsonResponse, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling AI Similar Products Service: {ex.Message}");
                return null;
            }
        }

        public async Task<ProductDetailsDTO> GetProductByIdAsync(int productId, string? userId)
        {
            var product = await _dbContext.Products
                .AsNoTracking()
                .Include(p => p.Seller)
                    .ThenInclude(s => s.University)
                .Include(p => p.ProductImages)
                .Include(p => p.Favorites)
                .Include(p => p.Categories)
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

        public async Task<PagedResult<ProductDetailListDTO>> GetCurrentUserProductAsync(string userId, int page = 1, int pageSize = 10, string status = null!)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            string[] validStatus = { "Pending", "Approved", "Rejected", "Sold" };

            if (status != null && !validStatus.Contains(status))
            {
                throw new ArgumentException("Trạng thái không hợp lệ!");
            }

            var query = _dbContext.Products
                .AsNoTracking()
                .Include(p => p.Favorites)
                .Include(p => p.Seller)
                .Include(p => p.ProductImages)
                .Include(p => p.Categories)
                .Where(p => p.SellerId == userId);

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(p => p.Status == status);
            }

            query = query.OrderByDescending(p => p.CreatedDate);

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

        public async Task<PagedResult<ProductListItemDTO>> GetUserProductPostsAsync(string userName, string? currentUserId, int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var user = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserName == userName);

            if (user == null)
            {
                throw new ArgumentException("Người dùng không tồn tại!");
            }

            var query = _dbContext.Products
                .AsNoTracking()
                .Include(p => p.Seller)
                .Include(p => p.ProductImages)
                .Include(p => p.Favorites)
                .Include(p => p.Categories)
                .Where(p => p.SellerId == user.Id && (p.Status == "Approved" || p.Status == "Sold"))
                .OrderByDescending(p => p.CreatedDate);

            var totalCount = await query.CountAsync();

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

            if (createProductPostDTO.ImageUrls == null || !createProductPostDTO.ImageUrls.Any())
            {
                throw new ArgumentException("Vui lòng thêm ít nhất một hình ảnh!");
            }

            if (createProductPostDTO.ImageUrls.Count > 6) // Adjust max limit as needed
            {
                throw new ArgumentException($"Số lượng hình ảnh không được vượt quá 6. Bạn đã tải lên {createProductPostDTO.ImageUrls.Count} hình ảnh.");
            }

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

            if (createProductPostDTO.ImageUrls?.Any() == true)
            {
                var productImages = new List<ProductImage>();
                foreach (var imageUrl in createProductPostDTO.ImageUrls)
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

            // ✅ Call AI service to update embeddings
            await UpdateProductEmbeddingAsync(product.ProductId, product.ProductName, product.ProductDescription, createProductPostDTO.CategoryIds, categories);
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
            var incomingImageUrls = createProductPostDTO.ImageUrls ?? new List<string>();

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

            // ✅ Call AI service to update embeddings
            await UpdateProductEmbeddingAsync(productId, product.ProductName, product.ProductDescription, createProductPostDTO.CategoryIds, categories);
        }

        public async Task ChangeProductStatusAsync(int productId, string userId, string status)
        {
            string[] validStatus = { "Approved", "Sold" };
            var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
            if (product == null)
            {
                throw new ArgumentException("Sản phẩm không tồn tại!");
            }
            if (product.SellerId != userId)
            {
                throw new ArgumentException("Sản phẩm không tồn tại!");
            }
            if (!validStatus.Contains(product.Status))
            {
                throw new ArgumentException("Trạng thái không hợp lệ!");
            }
            if (!validStatus.Contains(status))
            {
                throw new ArgumentException("Trạng thái không hợp lệ!");
            }
            product.Status = status;
            await _dbContext.SaveChangesAsync();
        }

        private async Task UpdateProductEmbeddingAsync(int productId, string productName, string? productDescription, List<int> categoryIds, List<Category> allCategories)
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _aiSettings.ApiKey);

            // Get the childest (leaf) category from the provided category IDs
            var selectedCategoryId = categoryIds.First();
            var childestCategory = allCategories.FirstOrDefault(c => c.CategoryId == selectedCategoryId);
            var childestCategoryName = childestCategory?.Name ?? "Khác";

            // Build the request body
            var requestBody = new
            {
                productName = productName,
                childestCategoryName = childestCategoryName,
                description = productDescription ?? string.Empty
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var aiUrl = $"{_aiSettings.BaseUrl}/update-embedding/{productId}";

            var response = await _httpClient.PutAsync(aiUrl, httpContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"⚠️ AI embedding sync failed for Product {productId}: {(int)response.StatusCode}. Chi tiết: {errorContent}");
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

        public async Task<ProductDetailsDTO> AdminGetProductAsync(int productId)
        {
            var product = await _dbContext.Products
                .AsNoTracking()
                .Include(p => p.Seller)
                .Include(p => p.ProductImages)
                .Include(p => p.Favorites)
                .Include(p => p.Categories)
                .FirstOrDefaultAsync(p => p.ProductId == productId);
            if (product == null)
            {
                throw new ArgumentException("Không tìm thấy sản phẩm!");
            }
            return product.ToProductDetailsDTO(null);
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

            //await _notificationService.SendProductNotificationAsync(userId, productName);
        }

        private async Task<AISearchResponseDTO?> CallAISearchServiceAsync(string search, int? categoryId, decimal? minPrice, decimal? maxPrice, int page, int pageSize)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("x-api-key", _aiSettings.ApiKey);

                var urlBuilder = new System.Text.StringBuilder();
                urlBuilder.Append($"{_aiSettings.BaseUrl}/search?q={Uri.EscapeDataString(search)}");
                urlBuilder.Append($"&page={page}&page_size={pageSize}");

                if (categoryId.HasValue)
                {
                    urlBuilder.Append($"&category_ids={categoryId.Value}");
                }

                if (minPrice.HasValue)
                {
                    urlBuilder.Append($"&min_price={minPrice.Value}");
                }

                if (maxPrice.HasValue)
                {
                    urlBuilder.Append($"&max_price={maxPrice.Value}");
                }

                var response = await _httpClient.GetAsync(urlBuilder.ToString());

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<AISearchResponseDTO>(jsonResponse, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling AI Search Service: {ex.Message}");
                return null;
            }
        }

        private IQueryable<Product> ApplySorting(IQueryable<Product> query, string sortBy)
        {
            return sortBy switch
            {
                "price_high" => query.OrderByDescending(p => p.Price),
                "price_low" => query.OrderBy(p => p.Price),
                "newest" => query.OrderByDescending(p => p.CreatedDate),
                _ => query.OrderByDescending(p => p.CreatedDate)
            };
        }

        private List<Product> SortProductList(List<Product> products, string sortBy)
        {
            return sortBy switch
            {
                "price_high" => products.OrderByDescending(p => p.Price).ToList(),
                "price_low" => products.OrderBy(p => p.Price).ToList(),
                "newest" => products.OrderByDescending(p => p.CreatedDate).ToList(),
                _ => products
            };
        }
    }
}
