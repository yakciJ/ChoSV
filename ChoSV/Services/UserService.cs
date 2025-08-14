using ChoSV.Data;
using ChoSV.Models.DTOs.User;
using ChoSV.Models.Entities;
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
    }
}
