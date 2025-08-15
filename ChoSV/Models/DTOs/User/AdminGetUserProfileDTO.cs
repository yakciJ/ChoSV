namespace ChoSV.Models.DTOs.User
{
    public class AdminGetUserProfileDTO
    {
        public required string UserId { get; set; }
        public required string UserName { get; set; }
        public required string UserEmail { get; set; }
        public bool EmailConfirmed { get; set; }
        public string? FullName { get; set; }
        public string? Bio { get; set; }
        public string? AvatarImage { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsBanned { get; set; }
        public int WarningCount { get; set; }
        public DateTime LastWarning { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
