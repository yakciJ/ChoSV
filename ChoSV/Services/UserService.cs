using ChoSV.Data;
using ChoSV.Models.DTOs.Common;
using ChoSV.Models.DTOs.User;
using ChoSV.Models.Entities;
using ChoSV.Models.Mappers;
using ChoSV.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ChoSV.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;
        private readonly ITokenService _tokenService;
        private readonly ApplicationDBContext _dbContext;
        private readonly SignInManager<User> _signInManager;
        public UserService(UserManager<User> userManager, IEmailService emailService, ITokenService tokenService, ApplicationDBContext dBContext, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _emailService = emailService;
            _tokenService = tokenService;
            _dbContext = dBContext;
            _signInManager = signInManager;
        }

        public async Task<bool> RegisterAsync(RegisterDTO registerDTO)
        {
            var user = new User
            {
                UserName = registerDTO.UserName,
                Email = registerDTO.Email,
                CreatedAt = DateTime.UtcNow,
            };

            var createUser = await _userManager.CreateAsync(user, registerDTO.Password);

            if (createUser.Succeeded)
            {
                var userRole = await _userManager.AddToRoleAsync(user, "User");
                if (userRole.Succeeded)
                {
                    var emailConfirmToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    await _emailService.SendConfirmEmailAsync(registerDTO.Email, registerDTO.UserName, emailConfirmToken);
                    return true;
                }
                else throw new ArgumentException("Thêm vai trò thất bại!");
            }
            else throw new ArgumentException(createUser.Errors.First().Code);
        }

        public async Task<LoginResponseDTO> LoginAsync(LoginDTO loginDTO)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == loginDTO.UserName);
            if (user == null)
            {
                throw new ArgumentException("Người dùng không tồn tại!");
            }
            if (user.EmailConfirmed == false)
            {
                throw new ArgumentException("Chưa xác thực Email, vui lòng xác thực Email để tiếp tục đăng nhập!");
            }
            if (user.IsBanned == true)
            {
                throw new ArgumentException("Tài khoản của bạn đã bị chặn!");
            }
            var res = await _signInManager.CheckPasswordSignInAsync(user, loginDTO.Password, true);
            if (res.IsLockedOut == true)
            {
                throw new ArgumentException("Tài khoản của bạn đã bị khóa vì nhập sai mật khẩu nhiều lần, hãy thử lại sau vài phút!");
            }
            if (!res.Succeeded)
            {
                throw new ArgumentException("Tài khoản hoặc mật khẩu không đúng!");
            }
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "User";
            var accessToken = await _tokenService.GenerateJwtToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            await _tokenService.SaveRefreshTokenAsync(loginDTO.RememberMe, user.Id, refreshToken, loginDTO.DeviceInfo, loginDTO.IpAddress);

            var loginResponse = new LoginResponseDTO
            {
                UserId = user.Id,
                UserName = loginDTO.UserName,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                AvatarImage = user.AvatarImage,
                Role = role,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
            return loginResponse;
        }

        public async Task ConfirmEmailAsync(string email, string token)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                throw new ArgumentException("Người dùng không tồn tại!");
            }
            var res = await _userManager.ConfirmEmailAsync(user, token);
            if (!res.Succeeded)
            {
                throw new ArgumentException("Xác thực người dùng thất bại!");
            }
        }

        public async Task<GetUserProfileDTO> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("Người dùng không tồn tại!");
            }
            return user.ToGetUserProfileDTO();
        }

        public async Task UpdateUserProfileAsync(string userId, UpdateUserDTO updateUserDTO)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => userId == u.Id);
            if (user == null)
            {
                throw new ArgumentException("Không tìm thấy người dùng!");
            }
            if (!string.IsNullOrEmpty(updateUserDTO.FullName))
                user.FullName = updateUserDTO.FullName;

            if (!string.IsNullOrEmpty(updateUserDTO.Bio))
                user.Bio = updateUserDTO.Bio;

            if (!string.IsNullOrEmpty(updateUserDTO.Address))
                user.Address = updateUserDTO.Address;

            if (!string.IsNullOrEmpty(updateUserDTO.PhoneNumber))
                user.PhoneNumber = updateUserDTO.PhoneNumber;

            var res = await _userManager.UpdateAsync(user);
            if (!res.Succeeded)
            {
                throw new ArgumentException("Cập nhập thông tin người dùng  thất bại!");
            }
        }

        public async Task DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("Người dùng không tồn tại!");
            }
            var res = await _userManager.DeleteAsync(user);
            if (!res.Succeeded)
            {
                throw new ArgumentException("Xóa người dùng thất bại!");
            }
            await _tokenService.RevokeRefreshTokenAsync(userId);
        }
        public async Task ChangePasswordAsync(string userId, ChangePasswordDTO changePasswordDTO)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("Người dùng không tồn tại!");
            }
            if (user.EmailConfirmed == false)
            {
                throw new ArgumentException("Chưa xác thực Email, vui lòng xác thực Email để tiếp tục!");
            }
            var res = await _userManager.ChangePasswordAsync(user, changePasswordDTO.OldPassword, changePasswordDTO.NewPassword);
            if (!res.Succeeded)
            {
                var errors = string.Join(", ", res.Errors.Select(e => e.Description));
                throw new ArgumentException($"Đổi mật khẩu thất bại: {errors}");
            }

            await _tokenService.RevokeAllUserRefreshTokensAsync(userId);
            await _emailService.SendChangedPasswordEmailAsync(user.Email!, user.UserName!);
        }
        public async Task ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return;
            }
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _emailService.SendForgotPasswordEmailAsync(email, user.UserName!, resetToken);
        }
        public async Task ResetPasswordAsync(ResetPasswordDTO resetPasswordDTO)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordDTO.Email);
            if (user == null)
            {
                throw new ArgumentException("Đặt lại mật khẩu thất bại!");
            }
            var res = await _userManager.ResetPasswordAsync(user, resetPasswordDTO.Token, resetPasswordDTO.NewPassword);
            if (!res.Succeeded)
            {
                var errors = string.Join(", ", res.Errors.Select(e => e.Description));
                throw new ArgumentException($"Đặt lại mật khẩu thất bại: {errors}");
            }
            await _tokenService.RevokeRefreshTokenAsync(user.Id);
            await _emailService.SendChangedPasswordEmailAsync(user.Email!, user.UserName!);
        }
        public async Task UpdateAvatarAsync(string userId, string avatarUrl)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => userId == u.Id);
            if (user == null)
            {
                throw new ArgumentException("Người dùng không tồn tại!");
            }
            user.AvatarImage = avatarUrl;
            await _dbContext.SaveChangesAsync();
        }

        // Admin func
        public async Task<PagedResult<AdminGetUserProfileDTO>> GetAllUsersByPageAsync(int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 1;
            if (pageSize > 100) pageSize = 100;

            int skip = (page - 1) * pageSize;

            var totalCount = await _dbContext.Users.CountAsync();

            var users = await _dbContext.Users.OrderBy(u => u.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            var userDTO = users.Select(u => u.ToAdminGetUserProfileDTO()).ToList();

            return new PagedResult<AdminGetUserProfileDTO>
            {
                Items = userDTO,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task BanOrUnbanAsync(string userId)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new ArgumentException("Người dùng không tồn tại!");
            }
            user.IsBanned = !user.IsBanned;
            if (user.IsBanned)
            {
                await _tokenService.RevokeAllUserRefreshTokensAsync(userId);
            }
            await _dbContext.SaveChangesAsync();
        }
    }
}
