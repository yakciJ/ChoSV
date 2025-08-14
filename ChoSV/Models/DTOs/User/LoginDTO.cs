using System.ComponentModel.DataAnnotations;

namespace ChoSV.Models.DTOs.User
{
    public class LoginDTO
    {
        [Required]
        public required string UserName { get; set; }

        [Required]
        public required string Password { get; set; }

        public string? DeviceInfo { get; set; }
        public string? IpAddress { get; set; }
        public bool RememberMe { get; set; } = false;
    }
}
