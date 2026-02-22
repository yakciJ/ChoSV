using ChoSV.Models.DTOs.Category;
using ChoSV.Models.DTOs.Product;
using ChoSV.Models.Entities;

namespace ChoSV.Models.Mappers
{
    public static class ProductMapper
    {
        public static ProductDetailsDTO ToProductDetailsDTO(this Product product, string? userId = null)
        {
            var parentCategory = product.Categories?.FirstOrDefault(c => c.ParentCategoryId == null);
            var childCategory = product.Categories?.FirstOrDefault(c => c.ParentCategoryId != null);

            return new ProductDetailsDTO
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                SellerId = product.SellerId,
                SellerName = product.Seller?.UserName ?? string.Empty,
                ProductDescription = product.ProductDescription ?? string.Empty,
                Price = product.Price,
                Status = product.Status,
                CreatedDate = product.CreatedDate,

                // Seller information
                SellerFullName = product.Seller?.FullName,
                SellerAvatarImage = product.Seller?.AvatarImage,
                SellerEmail = product.Seller?.Email,
                SellerAddress = product.Seller?.Address,
                SellerPhone = product.Seller?.PhoneNumber,
                SellerUniversity = product.Seller?.University?.UniversityName ?? string.Empty,
                SellerJoinedDate = product.Seller?.CreatedAt ?? DateTime.MinValue,

                // Product images
                ProductImages = product.ProductImages?.Select(pi => pi.ImageUrl).ToList() ?? new List<string>(),

                // Favorite count
                FavoriteCount = product.Favorites?.Count ?? 0,
                IsFavorite = !string.IsNullOrEmpty(userId) && (product.Favorites?.Any(f => f.UserId == userId) ?? false),

                // Category information
                ParentCategoryName = parentCategory?.Name,
                ChildCategoryName = childCategory?.Name,
                ParentCategoryId = parentCategory?.CategoryId,
                ChildCategoryId = childCategory?.CategoryId
            };
        }

        public static ProductListItemDTO ToProductListItemDTO(this Product product, string? userId = null)
        {
            return new ProductListItemDTO
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Price = product.Price,
                Status = product.Status,
                CreatedDate = product.CreatedDate,
                FirstImageUrl = product.ProductImages?.FirstOrDefault()?.ImageUrl,
                SellerName = product.Seller?.UserName ?? "Unknown",
                SellerFullName = product.Seller?.FullName ?? "",
                SellerAvatar = product.Seller?.AvatarImage ?? "",
                IsFavorited = !string.IsNullOrEmpty(userId) && product.Favorites.Any(f => f.UserId == userId),
                FavoriteCount = product.Favorites?.Count ?? 0,
                Categories = product.Categories?.Select(c => c.ToCategoryDTOFromCategory()).ToList() ?? new List<CategoryDTO>()
            };
        }

        public static ProductDetailListDTO ToProductDetailListDTO(this Product product)
        {
            return new ProductDetailListDTO
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                ProductPrice = product.Price,
                ProductDescription = product.ProductDescription,
                Status = product.Status,
                SellerId = product.SellerId,
                SellerName = product.Seller?.UserName ?? "Unknown",
                CreatedDate = product.CreatedDate,
                Categories = product.Categories?.Select(c => c.ToCategoryDTOFromCategory()).ToList() ?? new List<CategoryDTO>(),
                FirstImageUrl = product.ProductImages?.FirstOrDefault()?.ImageUrl
            };
        }

        public static void UpdateProductPost(this Product product, CreateProductPostDTO createProductPostDTO)
        {
            product.ProductName = createProductPostDTO.ProductName;
            product.Price = createProductPostDTO.Price;
            product.ProductDescription = createProductPostDTO.ProductDescription;
            product.Status = "Pending";
        }
    }
}
