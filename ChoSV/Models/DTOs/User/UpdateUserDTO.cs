using System.ComponentModel.DataAnnotations;

namespace ChoSV.Models.DTOs.User
{
    public class UpdateUserDTO
    {
        public string? FullName { get; set; }
        public string? Bio { get; set; }
        public string? Address { get; set; }
        [RegularExpression(@"^(?:\+84|0)[3|5|7|8|9]\d{8}$",
        ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? PhoneNumber { get; set; }
    }
}
