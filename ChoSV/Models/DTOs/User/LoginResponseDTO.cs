namespace ChoSV.Models.DTOs.User
{
    public class LoginResponseDTO
    {
        public required string UserId { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public string? FullName { get; set; }
        public string? AvatarImage { get; set; }
        public required string Role { get; set; }
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
    }
}
