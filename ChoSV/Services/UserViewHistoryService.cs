using ChoSV.Data;
using ChoSV.Models.DTOs.Category;
using ChoSV.Models.DTOs.Common;
using ChoSV.Models.DTOs.UserViewHistory;
using ChoSV.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChoSV.Services
{
    public class UserViewHistoryService : IUserViewHistory
    {
        private readonly ApplicationDBContext _dbContext;
        public UserViewHistoryService(ApplicationDBContext dBContext)
        {
            _dbContext = dBContext;
        }

        public async Task<PagedResult<GetUserViewHistories>> GetUserViewHistories(string userId, int page = 1, int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var historyQuery = _dbContext.UserViewHistories
                .AsNoTracking()
                .Where(vh => vh.UserId == userId)
                .OrderByDescending(vh => vh.LastViewedAt);

            var totalCount = await historyQuery.CountAsync();

            var viewHistories = await historyQuery
                .Include(vh => vh.Product)
                    .ThenInclude(p => p.Seller)
                .Include(vh => vh.Product)
                    .ThenInclude(p => p.ProductImages)
                .Include(vh => vh.Product)
                    .ThenInclude(p => p.Categories)
                .Include(vh => vh.Product)
                    .ThenInclude(p => p.Favorites)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var historyDTOs = viewHistories.Select(vh => new GetUserViewHistories
            {
                ProductId = vh.Product.ProductId,
                ProductName = vh.Product.ProductName,
                SellerId = vh.Product.SellerId,
                SellerName = vh.Product.Seller?.UserName ?? "Unknown",
                Price = vh.Product.Price,
                Status = vh.Product.Status,
                IsFavorited = vh.Product.Favorites?.Any(f => f.UserId == userId) ?? false,
                Categories = vh.Product.Categories?.Select(c => new CategoryDTO { CategoryId = c.CategoryId, CategoryName = c.Name }).ToList() ?? new List<CategoryDTO>(),
                FirstImageUrl = vh.Product.ProductImages?.FirstOrDefault()?.ImageUrl
            }).ToList();

            return new PagedResult<GetUserViewHistories>
            {
                Items = historyDTOs,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
        public async Task SawProduct(string userId, int productId)
        {
            await _dbContext.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""UserViewHistories"" (""UserId"", ""ProductId"", ""ViewCount"", ""LastViewedAt"")
                VALUES ({0}, {1}, 1, NOW() AT TIME ZONE 'UTC')
                ON CONFLICT (""UserId"", ""ProductId"")
                DO UPDATE SET 
                    ""ViewCount"" = ""UserViewHistories"".""ViewCount"" + 1,
                    ""LastViewedAt"" = NOW() AT TIME ZONE 'UTC';",
                userId, productId);
        }
        public async Task DeleteHistory(string userId, int productId)
        {
            var viewHistory = await _dbContext.UserViewHistories.FirstOrDefaultAsync(vh => vh.UserId == userId && vh.ProductId == productId);
            if (viewHistory == null)
            {
                throw new ArgumentException("Lịch sử không tồn tại!");
            }
            else
            {
                _dbContext.UserViewHistories.Remove(viewHistory);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task DeleteHistories(string userId)
        {
            var deletedCount = await _dbContext.UserViewHistories
                .Where(vh => vh.UserId == userId)
                .ExecuteDeleteAsync();
            if (deletedCount == 0)
            {
                throw new ArgumentException("Lịch sử không tồn tại!");
            }
        }
    }
}
