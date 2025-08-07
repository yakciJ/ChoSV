using System.ComponentModel.DataAnnotations;

namespace ChoSV.Models.DTOs.User
{
    public class RegisterDTO
    {
        [Required]
        public required string UserName { get; set; }
        [Required]
        public required string Password { get; set; }
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
    }
}
