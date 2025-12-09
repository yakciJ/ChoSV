using ChoSV.Models.DTOs.Common;
using ChoSV.Models.DTOs.User;

namespace ChoSV.Services.Interfaces
{
    public interface IUserService
    {
        Task<bool> RegisterAsync(RegisterDTO registerDTO);
        Task<LoginResponseDTO> LoginAsync(LoginDTO loginDTO);
        Task ConfirmEmailAsync(string email, string token);
        Task SendConfirmEmailAsync(string email);
        Task<string> GetAccessTokenAsync(string refreshToken);
        Task<GetUserProfileDTO> GetUserByUserNameAsync(string userName);
        Task UpdateUserProfileAsync(string userId, UpdateUserDTO updateUserDTO);
        Task DeleteUserAsync(string userId);
        Task ChangePasswordAsync(string userId, ChangePasswordDTO changePasswordDTO);
        Task ForgotPasswordAsync(string email);
        Task ResetPasswordAsync(ResetPasswordDTO resetPasswordDTO);
        Task UpdateAvatarAsync(string userId, string avatarUrl);

        Task<PagedResult<AdminGetUserProfileDTO>> GetAllUsersByPageAsync(int page, int pageSize = 10);
        Task BanOrUnbanAsync(string userId);
        // xem thông tin cá nhân và của người dùng khác là chung. sau thêm được cái setting hiển thị thông tin hay không thì sẽ làm riêng sau.
        // về admin: lấy thông tin chi tiết người dùng? nếu cần tại cũng gần giống của user rồi,  cảnh cáo người dùng, xóa cảnh cáo,...
        // phần cảnh cáo còn phải gửi noti đến người dùng và email nữa, hơi lằng nhằng, chắc làm sau khi làm xong noti.
        // à còn tìm kiếm người dùng nữa. thế khác gì xem thông tin public nhỉ? tìm gần giống là được à
        // thêm các service và endpoint sau: optional: thêm thông tin chỉ số người dùng, ví dụ như bao nhiêu mặt hàng đã bán, bao nhiêu like, bao nhiêu yêu thích, chắc nên vứt hết vào 2 cái xem thông tin ở trên.
    }
}
