using ChoSV.Models.DTOs.User;

namespace ChoSV.Services.Interfaces
{
    public interface IUserService
    {
        Task<bool> RegisterAsync(RegisterDTO registerDTO);
        Task<LoginResponseDTO> LoginAsync(LoginDTO loginDTO);
        Task ConfirmEmailAsync(string email, string token);
        Task UpdateUserProfileAsync(string userId, UpdateUserDTO updateUserDTO);
        Task DeleteUserAsync(string userId);
        Task ChangePasswordAsync(string userId, ChangePasswordDTO changePasswordDTO);
        Task ForgotPasswordAsync(string email);
        Task ResetPasswordAsync(ResetPasswordDTO resetPasswordDTO);
        Task UpdateAvatarAsync(string userId, string avatarUrl);

        // thêm các service và endpoint sau: xem thông tin cá nhân, xem thông tin của người dùng khác, thêm endpoint đổi avatar, optional: thêm thông tin chỉ số người dùng, ví dụ như bao nhiêu mặt hàng đã bán, bao nhiêu like, bao nhiêu yêu thích, chắc nên vứt hết vào 2 cái xem thông tin ở trên.
        // về admin: thêm lấy toàn bộ người dùng để làm trang quản lý, lấy thông tin chi tiết người dùng? nếu cần tại cũng gần giống của user rồi, chặn người dùng, cảnh cáo người dùng, xóa cảnh cáo, bỏ chặn,...
        // à còn tìm kiếm người dùng nữa. thế khác gì xem thông tin public nhỉ? tìm gần giống là được à
    }
}
