using ChoSV.Models.DTOs.Common;
using ChoSV.Models.DTOs.UserViewHistory;

namespace ChoSV.Services.Interfaces
{
    public interface IUserViewHistory
    {
        Task<PagedResult<GetUserViewHistories>> GetUserViewHistories(string userId, int page = 1, int pageSize = 20);
        Task SawProduct(string userId, int productId);
        Task DeleteHistory(string userId, int productId);
        Task DeleteHistories(string userId);
    }
}
