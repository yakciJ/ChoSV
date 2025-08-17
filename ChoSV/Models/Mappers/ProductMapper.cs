using ChoSV.Models.DTOs.Product;
using ChoSV.Models.Entities;

namespace ChoSV.Models.Mappers
{
    public static class ProductMapper
    {
        public static ProductDetailsDTO ToProductDetailsDTO(this Product product, string? userId = null)
        {
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
                SellerJoinedDate = product.Seller?.CreatedAt ?? DateTime.MinValue,

                // Product images
                ProductImages = product.ProductImages?.Select(pi => pi.ImageUrl).ToList() ?? new List<string>(),

                // Favorite count
                FavoriteCount = product.Favorites?.Count ?? 0,
                IsFavorite = !string.IsNullOrEmpty(userId) && (product.Favorites?.Any(f => f.UserId == userId) ?? false)
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
                SellerName = product.Seller?.UserName ?? "Unknown",
                FirstImageUrl = product.ProductImages?.FirstOrDefault()?.ImageUrl,
                IsFavorited = !string.IsNullOrEmpty(userId) && product.Favorites.Any(f => f.UserId == userId),
                FavoriteCount = product.Favorites?.Count ?? 0
            };
        }
    }
}
