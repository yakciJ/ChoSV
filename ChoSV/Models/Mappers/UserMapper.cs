using ChoSV.Models.DTOs.User;
using ChoSV.Models.Entities;

namespace ChoSV.Models.Mappers
{
    public static class UserMapper
    {
        public static GetUserProfileDTO ToGetUserProfileDTO(this User user)
        {
            return new GetUserProfileDTO
            {
                UserId = user.Id,
                UserName = user.UserName!,
                FullName = user.FullName,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber,
                AvatarImage = user.AvatarImage,
                Bio = user.Bio,
                Address = user.Address,
                CreatedAt = user.CreatedAt
            };
        }

        public static AdminGetUserProfileDTO ToAdminGetUserProfileDTO(this User user)
        {
            return new AdminGetUserProfileDTO
            {
                UserId = user.Id,
                UserName = user.UserName!,
                UserEmail = user.Email!,
                EmailConfirmed = user.EmailConfirmed,
                FullName = user.FullName,
                Bio = user.Bio,
                AvatarImage = user.AvatarImage,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber,
                IsBanned = user.IsBanned,
                WarningCount = user.WarningCount,
                LastWarning = user.LastWarning,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
