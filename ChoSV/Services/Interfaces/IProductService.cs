using ChoSV.Models.DTOs.Product;

namespace ChoSV.Services.Interfaces
{
    public interface IProductService
    {
        Task<ProductDetailsDTO> GetProductByIdAsync(int productId, string? userId);
        Task CreateProductPostAsync(string userId, CreateProductPostDTO createProductPostDTO);
    }
    // Các trạng thái của product:
    //  Pending: Chờ duyệt(khi người dùng vừa đăng).
    //  Approved: Đã duyệt(hiện công khai cho mọi người xem/mua).
    //  Rejected: Bị từ chối(vi phạm quy định hoặc nội dung không hợp lệ).
    //  Sold: Sản phẩm đã bán xong.
    //  Archived / Deleted: Bài đăng đã ẩn hoặc bị xóa.
}
