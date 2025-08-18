using ChoSV.Data;
using ChoSV.Models.DTOs.Common;
using ChoSV.Models.DTOs.Product;
using ChoSV.Models.Entities;
using ChoSV.Models.Mappers;
using ChoSV.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChoSV.Services
{
    public class FavoriteService : IFavoriteService
    {
        private readonly ApplicationDBContext _dbContext;
        public FavoriteService(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedResult<ProductListItemDTO>> GetAllFavoriteProductsAsync(string userId, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var favoriteQuery = _dbContext.Favorites
                .AsNoTracking()
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt);

            var totalCount = await favoriteQuery.CountAsync();

            var favorites = await favoriteQuery
                .Include(f => f.Product)
                    .ThenInclude(p => p.ProductImages)
                .Include(f => f.Product)
                    .ThenInclude(p => p.Categories)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var productDTOs = favorites.Select(f => f.Product.ToProductListItemDTO()).ToList();

            return new PagedResult<ProductListItemDTO>
            {
                Items = productDTOs,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task AddFavoriteAsync(int productId, string userId)
        {
            var product = await _dbContext.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null)
            {
                throw new ArgumentException("Sản phẩm không tồn tại!");
            }

            var existingFavorite = await _dbContext.Favorites
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.ProductId == productId && f.UserId == userId);

            if (existingFavorite != null)
            {
                throw new ArgumentException("Sản phẩm đã được yêu thích!");
            }

            var favorite = new Favorite
            {
                UserId = userId,
                ProductId = productId
            };
            _dbContext.Favorites.Add(favorite);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteFavoriteAsync(int productId, string userId)
        {
            var favorite = await _dbContext.Favorites
                .FirstOrDefaultAsync(f => f.ProductId == productId && f.UserId == userId);

            if (favorite == null)
            {
                throw new ArgumentException("Chưa yêu thích, không thể xóa!");
            }

            _dbContext.Favorites.Remove(favorite);
            await _dbContext.SaveChangesAsync();
        }
    }
}
