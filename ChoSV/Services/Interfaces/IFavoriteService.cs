using ChoSV.Models.DTOs.Common;
using ChoSV.Models.DTOs.Product;

namespace ChoSV.Services.Interfaces
{
    public interface IFavoriteService
    {
        Task<PagedResult<ProductListItemDTO>> GetAllFavoriteProductsAsync(string userId, int page, int pageSize);
        Task AddFavoriteAsync(int productId, string userId);
        Task DeleteFavoriteAsync(int productId, string userId);
    }
}
