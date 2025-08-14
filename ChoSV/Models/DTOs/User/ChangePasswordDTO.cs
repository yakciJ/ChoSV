using System.ComponentModel.DataAnnotations;

namespace ChoSV.Models.DTOs.User
{
    public class ChangePasswordDTO
    {
        [Required(ErrorMessage = "Mật khẩu cũ là bắt buộc")]
        public required string OldPassword { get; set; }

        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự")]
        public required string NewPassword { get; set; }

        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public required string ConfirmPassword { get; set; }
    }
}
