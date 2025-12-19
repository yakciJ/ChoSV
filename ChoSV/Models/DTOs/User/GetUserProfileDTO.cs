namespace ChoSV.Models.DTOs.User
{
    public class GetUserProfileDTO
    {
        public required string UserId { get; set; }
        public required string UserName { get; set; }
        public string? FullName { get; set; }
        public required string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarImage { get; set; }
        public string? Bio { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
