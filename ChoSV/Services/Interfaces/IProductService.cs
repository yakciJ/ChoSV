using ChoSV.Models.DTOs.Common;
using ChoSV.Models.DTOs.Product;

namespace ChoSV.Services.Interfaces
{
    public interface IProductService
    {
        Task<ProductDetailsDTO> GetProductByIdAsync(int productId, string? userId);
        Task<PagedResult<ProductDetailListDTO>> GetCurrentUserProductAsync(string userId, int page = 1, int pageSize = 10);
        Task<PagedResult<ProductListItemDTO>> GetUserProductPostsAsync(string userId, string? currentUserId, int page = 1, int pageSize = 10);
        Task CreateProductPostAsync(string userId, CreateProductPostDTO createProductPostDTO);
        Task UpdateProductPostAsync(string userId, int productId, CreateProductPostDTO updateProductPostDTO);
        Task DeleteProductPostAsync(string userId, int productId);

        Task<PagedResult<ProductDetailListDTO>> AdminGetAllProductAsync(int page = 1, int pageSize = 10, string? status = null);
        Task AdminUpdateProductStatusAsync(int productId, string status);
        Task AdminDeleteProductPostAsync(int productId);
    }
    // Các trạng thái của product:
    //  Pending: Chờ duyệt(khi người dùng vừa đăng).
    //  Approved: Đã duyệt(hiện công khai cho mọi người xem/mua).
    //  Rejected: Bị từ chối(vi phạm quy định hoặc nội dung không hợp lệ).
    //  Sold: Sản phẩm đã bán xong.
    //  Archived / Deleted: Bài đăng đã ẩn hoặc bị xóa.
    // chắc là bỏ cái deleted đi nhỉ, tại xóa thì xóa luôn trong db cho nhanh.

    // Nhớ khi làm frontend: Khi người dùng upload ảnh lên mà k submit product (create or update) nhớ gọi api xóa đống đấy đi cho đỡ rác.

    // Rảnh chó thì làm thêm phần valid url ảnh.
}
